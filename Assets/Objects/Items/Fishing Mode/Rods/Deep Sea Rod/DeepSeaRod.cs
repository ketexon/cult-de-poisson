using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator), typeof(LineRenderer))]
public class DeepSeaRod : FishingRodV2
{
    enum State
    {
        Uncast,
        Casting,
        Cast,
        Hooked,
        Broken,
    }

    enum HookedState
    {
        Default,
        FishTug,
        PlayerTug,
        FishTugSnap,
    }

    [SerializeField] GameObject tip;

    [SerializeField] float maxLineLength = 10;

    [Tooltip("Tension half life (seconds)")]
    [SerializeField] float tensionHalfLife = 4.0f;
    [SerializeField] float maxTension = 5.0f;
    [SerializeField] float lineReturnDuration = 1.0f;

    State state = State.Uncast;
    HookedState hookedState = HookedState.Default;

    Animator animator;
    LineRenderer lineRenderer;

    ConfigurableJoint hookJoint;

    Vector3[] linePositions = new Vector3[2];

    Fish fish;
    HookedFish hookedFish;

    float fishStrength = 0;
    float tension = 0;
    float minLineLength;
    float lineLength;

    /// <summary>
    /// The length of the line when the 
    /// return animation starts.
    /// Used in the first parameter for the lerp
    /// </summary>
    float lineReturnStartLength;

    /// <summary>
    /// The start time for the return animation
    /// Used for the time parameter of the lerp
    /// </summary>
    float lineReturnStartTime;

    bool fishCollectable = false;

    System.Action inputUIDestructor = null;

    public override bool CanExitFishingMode => state == State.Uncast && base.CanExitFishingMode;

    override protected void Awake()
    {
        base.Awake();

        animator = GetComponent<Animator>();
        lineRenderer = GetComponent<LineRenderer>();

        hookJoint = Hook.GetComponent<ConfigurableJoint>();

        Hook.OnHook += OnFishHooked;

        minLineLength = hookJoint.linearLimit.limit;
        lineLength = minLineLength;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Hook.OnHook -= OnFishHooked;
    }

    /// <summary>
    /// Called when the cast key is used.
    /// Used to both cast the line and to 
    /// uncast the line
    /// </summary>
    public override void Cast()
    {
        if (state == State.Uncast)
        {
            state = State.Casting;
            animator.SetTrigger("Cast");
        }
        else if(state == State.Cast)
        {
            ReturnLine();
            ResetFishing();
        }
    }

    /// <summary>
    /// When the FISHING ROD is interacted fish. This is called by PlayerInteract
    /// when the player presses INTERACT on the fishing rod (used to start casting)
    /// Compare <see cref="OnInputInteract"/>, which is the Fishing Rod's own
    /// interact behaviour, used to pick up fish.
    /// </summary>
    public override void OnInteract()
    {
        base.OnInteract();

        FishingModeItem.Phase = FishingModePhase.Fishing;
        state = State.Casting;
        animator.SetTrigger("Cast");

        collider.enabled = false;
    }

    override protected void Update()
    {
        base.Update();

        UpdateLineRenderer();

        if(state == State.Hooked)
        {
            // apply decay to tension
            // decay equation: dN/dt = -1/t_h * N
            // where t_h is half life
            tension -= tension / tensionHalfLife * Time.deltaTime;
            //Debug.Log($"Tension: {tension}");
            if (tension > maxTension)
            {
                Break();
            }
        }
        if(state == State.Broken || state == State.Uncast)
        {
            if(Time.time < lineReturnStartTime + lineReturnDuration)
            {
                UpdateLineLength(Mathf.Lerp(lineReturnStartLength, minLineLength, (Time.time - lineReturnStartTime)/lineReturnDuration));
            }
        }
    }

    /// <summary>
    /// Draws a line between the tip of the rod and the hook
    /// </summary>
    void UpdateLineRenderer()
    {
        linePositions[0] = tip.transform.position;
        linePositions[1] = Hook.transform.position;

        lineRenderer.SetPositions(linePositions);
        lineRenderer.positionCount = linePositions.Length;
    }

    /// <summary>
    /// Called by animator trigger when the cast animation is done
    /// </summary>
    void OnFinishCasting()
    {
        if (state != State.Casting) return;
        state = State.Cast;
        // Remove the distance limit on the hook (ie. drop the line)
        UpdateLineLength(maxLineLength);

        // make the hook visible to fish
        Hook.Visible = true;
    }

    /// <summary>
    /// Called when a fish invokes the OnHook event on the FishingHook
    /// Happens when the fish is hooked.
    /// </summary>
    /// <param name="fish">Fish that collided with the hook</param>
    void OnFishHooked(Fish fish)
    {
        if (state != State.Cast) return;

        state = State.Hooked;
        this.fish = fish;
        
        hookedFish = fish.GetComponent<HookedFish>();
        
        hookedFish.RodTipTransform = tip.transform;
        hookedFish.PlayerTransform = FishingModeItem.Player.transform;

        hookedFish.TugEvent += OnFishTug;

        tugAction.action.performed += OnPlayerTug;
        interactAction.action.performed += OnInputInteract;
    }

    void OnFishUnhooked()
    {
        hookedFish.TugEvent -= OnFishTug;

        tugAction.action.performed -= OnPlayerTug;
        interactAction.action.performed -= OnInputInteract;

        fish = null;
        hookedFish = null;
    }

    /// <summary>
    /// Called when interact key is pressed, used to primarily collect
    /// fish at end of fishing.
    /// Compare with <see cref="OnInteract"/>, which is used to start fishing,
    /// called from the base class <see cref="Interactable"/>
    /// </summary>
    /// <param name="ctx"></param>
    protected void OnInputInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (fishCollectable)
        {
            inputUIDestructor?.Invoke();
            inputUIDestructor = null;
            FishingModeItem.CollectFish(fish);
        }
    }

    void Break()
    {
        //float random = Random.Range(0.0f, 1.0f);

        animator.SetTrigger("Break");

        hookedFish.UnhookEvent?.Invoke();
        OnFishUnhooked();

        Hook.Break();

        ReturnLine();
        state = State.Broken;
    }

    void OnFishTug(float strength)
    {
        fishStrength = strength;

        // reset playertug trigger just in case
        animator.ResetTrigger("PlayerTug");

        hookedState = HookedState.FishTug;
        animator.SetFloat("FishTugSpeedMult", 1 + Mathf.Log10(strength + 1));
        animator.SetTrigger("FishTug");
    }

    void OnPlayerTug(InputAction.CallbackContext _)
    {
        // if the fish tugged
        if (hookedState == HookedState.FishTug)
        {
            hookedState = HookedState.Default;
            animator.SetTrigger("PlayerTug");
        }
    }

    /// <summary>
    /// Triggered by tug animation clip when it reaches the "critical point"
    /// If the player does not tug at this point, then the rod has a chance
    /// to snap, etc.
    /// </summary>
    void TugSnap()
    {
        // if the player has not tugged back
        if(hookedState == HookedState.FishTug)
        {
            hookedState = HookedState.Default;
            tension += fishStrength;
        }
    }

    protected override void Reel(float amount)
    {
        if (state != State.Hooked) return;
        if (hookedState == HookedState.Default)
        {
            UpdateLineLength(lineLength - amount);
        }
    }

    void UpdateLineLength(float value)
    {
        lineLength = Mathf.Clamp(value, minLineLength, maxLineLength);
        hookJoint.linearLimit = new()
        {
            limit = lineLength,
            bounciness = hookJoint.linearLimit.bounciness,
            contactDistance = hookJoint.linearLimit.contactDistance,
        };

        if(lineLength < CollectableLineLength && fish)
        {
            fishCollectable = true;
            inputUIDestructor?.Invoke();
            inputUIDestructor = InputUI.Instance.AddInputUI(interactAction.action, "Collect fish");
        }
    }

    void ReturnLine()
    {
        lineReturnStartTime = Time.time;
        lineReturnStartLength = lineLength;
    }

    override public void ResetFishing()
    {
        fish = null;
        hookedFish = null;
        fishCollectable = false;

        animator.SetTrigger("Reset");

        state = State.Uncast;
        FishingModeItem.Phase = FishingModePhase.Prepping;

        collider.enabled = false;

        Hook.ResetHook();
    }
}
