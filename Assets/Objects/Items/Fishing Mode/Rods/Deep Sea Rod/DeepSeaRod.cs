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
    }

    enum HookedState
    {
        Default,
        FishTug,
        PlayerTug,
        FishTugSnap,
    }

    [SerializeField] GameObject tip;

    [SerializeField] float lineLength = 10;

    State state = State.Uncast;
    HookedState hookedState = HookedState.Default;

    Animator animator;
    LineRenderer lineRenderer;

    ConfigurableJoint hookJoint;

    Vector3[] linePositions = new Vector3[2];

    Fish fish;
    HookedFish hookedFish;

    override protected void Awake()
    {
        base.Awake();

        animator = GetComponent<Animator>();
        lineRenderer = GetComponent<LineRenderer>();

        hookJoint = Hook.GetComponent<ConfigurableJoint>();

        Hook.OnHook += OnFishHooked;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Hook.OnHook -= OnFishHooked;
    }

    /// <summary>
    /// Called when the cast key is used
    /// </summary>
    public override void Cast()
    {
        if (state != State.Uncast) return;
        state = State.Casting;
        animator.SetTrigger("Cast");
    }

    void Update()
    {
        UpdateLineRenderer();
    }

    /// <summary>
    /// Draws a line between the tip of the rod and the hook
    /// </summary>
    void UpdateLineRenderer()
    {
        linePositions[0] = tip.transform.position;
        linePositions[1] = Hook.transform.position;

        lineRenderer.SetPositions(linePositions);
    }

    /// <summary>
    /// Called by animator trigger when the cast animation is done
    /// </summary>
    void OnFinishCasting()
    {
        if (state != State.Casting) return;
        state = State.Cast;
        // Remove the distance limit on the hook (ie. drop the line)
        hookJoint.linearLimit = new()
        {
            limit = lineLength,
            bounciness = hookJoint.linearLimit.bounciness,
            contactDistance = hookJoint.linearLimit.contactDistance,
        };
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
        hookedFish.TugEvent += OnFishTug;
        tugAction.action.performed += OnPlayerTug;
    }

    void OnFishTug(float strength)
    {
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
            hookedState = HookedState.PlayerTug;
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
            hookedState = HookedState.FishTugSnap;
        }
    }
}
