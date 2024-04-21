using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class FishingRod : Item
{
    // only allow switching items if there is no hooked fish
    // (ie. force player to pick up/drop fish)
    public override bool CanSwitchItems => hookedFish == null;

    enum FishingState
    {
        Uncast,
        Cast,
        Caught,
    }

    [SerializeField] GlobalParametersSO parameters;


    [SerializeField] Transform rod;
    [SerializeField] Transform rodCastTransform;
    /// <summary>
    /// The tip of the fishing rod where the fishing line is cast from
    /// </summary>
    [SerializeField] Transform rodTipTransform;
    
    [SerializeField] FishingHook hook;
    [SerializeField] float fishingSensitivityY = 0.05f;
    [SerializeField] float fishingSensitivityX = 0.05f;
    [SerializeField] FishingLine fishingLine;

    [SerializeField] ItemSO fishItemSO;
    [SerializeField] PlayerInventorySO inventory;

    [SerializeField] InputActionReference aimAction;
    [SerializeField] InputActionReference unaimAction;
    [SerializeField] InputActionReference moveAction;
    [SerializeField] InputActionReference clickAction;
    [SerializeField] InputActionReference reelAction;

    FishingState fishingState = FishingState.Uncast;
    bool aiming = false;

    /// <summary>
    /// The initial rotation of the fishing rod when 
    /// we start fishing.
    /// </summary>
    Quaternion rodFishingStartRot;

    Quaternion uncastLocalRotation;

    /// <summary>
    /// The rodTipPos from last frame.
    /// Used to calculate velocity.
    /// </summary>
    Vector3 lastRodTipPos;
    Vector3 rodTipVelocity;

    /// <summary>
    /// The current rotation of the fishing rod while in fishing mode.
    /// Given to <see cref="PlayerItem"/> to automatically lerp the fishing rod.
    /// </summary>
    float rodFishingTargetAngle;

    /// <summary>
    /// The current position x value of the fishing rod when in fishing mode.
    /// Given to <see cref="PlayerItem"/> to automatically lerp the fishing rod.
    /// </summary>
    float rodFishingTargetX;

    float reelStrength = 0;

    /// <summary>
    /// If the hook is in range to either pick up a hooked fish or reset fishing.
    /// </summary>
    bool hookInRange = false;

    Fish fish;
    HookedFish hookedFish;

    /// <summary>
    /// Callback coming from <see cref="InputUI"/> to remove the UI from screen.
    /// </summary>
    System.Action inputUIDestructor = null;

    protected virtual void OnEnable()
    {
        aimAction.action.performed += OnAim;
        unaimAction.action.canceled += OnReleaseAim;

        moveAction.action.performed += OnFishMove;
        moveAction.action.canceled += OnFishMove;

        clickAction.action.performed += OnFishClick;
        reelAction.action.performed += OnFishReel;

        if (InputUI.Instance)
        {
            UpdateInputUI();
        }
    }

    void Awake()
    {
        lastRodTipPos = rodTipTransform.position;

        uncastLocalRotation = rod.localRotation;
    }

    void Start()
    {
        UpdateInputUI();
    }

    void Update()
    {
        var rodTipPos = rodTipTransform.position;
        rodTipVelocity = (rodTipPos - lastRodTipPos) / Time.deltaTime;
        lastRodTipPos = rodTipPos;

        if (!aiming)
        {
            if (fishingState == FishingState.Cast)
            {
                rod.localRotation = Quaternion.Lerp(
                    rod.localRotation,
                    rodCastTransform.localRotation,
                    Time.deltaTime * playerItem.RotateSpeed
                );
            }
            else if (fishingState == FishingState.Uncast)
            {
                rod.localRotation = Quaternion.Lerp(
                    rod.localRotation,
                    uncastLocalRotation,
                    Time.deltaTime * playerItem.RotateSpeed
                );
            }
        }
    }


    protected virtual void OnDisable()
    {
        inputUIDestructor?.Invoke();
        inputUIDestructor = null;

        if (fish && hookInRange)
        {
            inventory.AddFish(fish.FishSO);
        }
        ResetFishing();

        // hard reset local rotation of fishing rod
        rod.localRotation = uncastLocalRotation;

        // unregister actions
        aimAction.action.performed -= OnAim;
        unaimAction.action.canceled -= OnReleaseAim;

        moveAction.action.performed -= OnFishMove;
        moveAction.action.canceled -= OnFishMove;

        clickAction.action.performed -= OnFishClick;
        reelAction.action.performed -= OnFishReel;
    }

    void Reset()
    {
        parameters = FindUtil.Asset<GlobalParametersSO>();
        playerInput = GetComponentInParent<PlayerInput>();
        rodTipTransform = FindUtil.Query<Transform>(this)
            .InChildren
            .NameContains("tip", insensitive: true)
            .Execute();
    }

    void OnAim(InputAction.CallbackContext ctx)
    {
        aiming = true;

        playerInput.SwitchCurrentActionMap("Fishing");

        // store the initial rotation of the rod
        // and reset the angle and x position offsets
        rodFishingStartRot = playerItem.TargetRot;
        rodFishingTargetAngle = 0;
        rodFishingTargetX = 0;

        // we need to rotate the player item ourselves
        // so we stop it from automatically rotating
        playerItem.SetRotationLock(false);

        InteractivityChangeEvent?.Invoke(this);

        UpdateInputUI();
    }

    void OnReleaseAim(InputAction.CallbackContext ctx)
    {
        aiming = false;

        playerInput.SwitchCurrentActionMap("Gameplay");

        // we need to rotate the player item ourselves
        // so we stop it from automatically rotating
        playerItem.SetRotationLock(true);

        InteractivityChangeEvent?.Invoke(this);

        UpdateInputUI();
    }


    public void OnFishMove(InputAction.CallbackContext ctx)
    {
        var delta = ctx.ReadValue<Vector2>();
        rodFishingTargetAngle = Mathf.Clamp(
            rodFishingTargetAngle + delta.y * fishingSensitivityY,
            -120, 30
        );

        rodFishingTargetX = Mathf.Clamp(
            rodFishingTargetX + delta.x * fishingSensitivityX,
            -10, 10
        );

        playerItem.TargetRot = rodFishingStartRot * Quaternion.Euler(rodFishingTargetAngle, rodFishingTargetX, 0);
    }

    public void OnFishClick(InputAction.CallbackContext ctx)
    {
        if(fishingState == FishingState.Uncast)
        {
            Cast();
        }
    }

    public void OnFishReel(InputAction.CallbackContext ctx)
    {
        reelStrength = Mathf.Clamp(-ctx.ReadValue<float>() / 120, -1, 1);
        fishingLine.Reel(reelStrength);
    }

    #region Interaction
    public override bool TargetInteractVisible => hookInRange;
    public override bool TargetInteractEnabled => !aiming;
    public override string TargetInteractMessage => fish ? "Collect Fish" : "Reset Fishing";

    override public void OnInteract()
    {
        if (fish)
        {
            // save hooked fish, since enabling another item
            // calls OnDisable, which sets hookedFish to null
            var hookedFish = this.fish;

            // note: this will disable the current script, and hence
            // add the hooked fish to the inventory
            var fishItem = playerItem.GetItem(fishItemSO, true) as FishItem;
            fishItem.SetFish(hookedFish);
            playerItem.EnableItem(fishItem, true);

            InteractivityChangeEvent?.Invoke(this);
        }
        else
        {
            ResetFishing();
            InteractivityChangeEvent?.Invoke(this);
        }
    }
    #endregion

    /// <summary>
    /// Spawns a hook with the same velocity as the rod tip.
    /// Spawns a fishing line and attach it to the hook.
    /// </summary>
    void Cast()
    {
        var hookRB = hook.GetComponent<Rigidbody>();

        hook.transform.position = rodTipTransform.position;
        hookRB.velocity = rodTipVelocity;

        hook.OnHook += OnHookFish;
        hook.gameObject.SetActive(true);

        fishingLine.enabled = true;
        fishingLine.OnCast(hook, rodTipTransform, rodTipVelocity);

        fishingState = FishingState.Cast;
    }

    /// <summary>
    /// Reset internal fishing state and destroy 
    /// non-child GameObjects (fishing line and hook).
    /// </summary>
    void ResetFishing()
    {
        fishingState = FishingState.Uncast;


        fish = null;
        hookedFish = null;
        hookInRange = false;

        if (hook)
        {
            hook.gameObject.SetActive(false);
        }
        fishingLine.enabled = false;

        InteractivityChangeEvent?.Invoke(this);
    }

    /// <summary>
    /// Called by <see cref="FishingHookV2"/> to set the <see cref="hookInRange"/> member.
    /// </summary>
    /// <param name="inRange"></param>
    public void SetHookInRange(bool inRange)
    {
        if(hookInRange != inRange)
        {
            hookInRange = inRange;
            InteractivityChangeEvent?.Invoke(this);
        }
    }

    void UpdateInputUI()
    {
        inputUIDestructor?.Invoke();
        if (aiming)
        {
            inputUIDestructor = null;
        }
        else
        {
            inputUIDestructor = InputUI.Instance.AddInputUI(
                aimAction,
                "Aim fishing rod"
            );
        }
    }

    public void OnHookFish(Fish fish)
    {
        this.fish = fish;

        hookedFish = fish.GetComponent<HookedFish>();
        hookedFish.PlayerTransform = player.transform;
        hookedFish.RodTipTransform = rodTipTransform;
    }
}
