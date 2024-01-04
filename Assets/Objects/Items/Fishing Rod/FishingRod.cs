using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.ParticleSystem;

public class FishingRod : Item
{
    enum FishingState
    {
        Uncast,
        Cast,
        Caught,
    }

    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] Transform rodTipTransform;
    [SerializeField] GameObject hookPrefab;
    [SerializeField] float fishingSensitivityY = 0.05f;
    [SerializeField] float fishingSensitivityX = 0.05f;
    [SerializeField] GameObject fishingLinePrefab;

    [SerializeField] ItemSO fishItem;
    [SerializeField] PlayerInventorySO inventory;

    [SerializeField] InputActionReference interactAction;
    [SerializeField] InputActionReference moveAction;
    [SerializeField] InputActionReference clickAction;
    [SerializeField] InputActionReference reelAction;
    [SerializeField] InputActionReference exitAction;

    GameObject hookGO;
    FishingLine fishingLine;

    FishingState fishingState = FishingState.Uncast;

    Quaternion rodFishingStartRot;

    Vector3 lastRodTipPos;
    Vector3 rodTipVelocity;

    float rodFishingTargetAngle;
    float rodFishingTargetX;

    float reelStrength = 0;

    bool hookInRange = false;

    Fish hookedFish;

    System.Action inputUIDestructor = null;

    public override void Initialize(InitializeParams initParams)
    {
        base.Initialize(initParams);

        moveAction.action.performed += OnFishMove;
        moveAction.action.canceled += OnFishMove;

        clickAction.action.performed += OnFishClick;
        reelAction.action.performed += OnFishReel;
        exitAction.action.performed += OnExitFishing;
    }

    void OnDestroy()
    {
        moveAction.action.performed -= OnFishMove;
        moveAction.action.canceled -= OnFishMove;

        clickAction.action.performed -= OnFishClick;
        reelAction.action.performed -= OnFishReel;
        exitAction.action.performed -= OnExitFishing;
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

    void Awake()
    {
        lastRodTipPos = rodTipTransform.position;
    }

    void Update()
    {
        var rodTipPos = rodTipTransform.position;
        rodTipVelocity = (rodTipPos - lastRodTipPos) / Time.deltaTime;
        lastRodTipPos = rodTipPos;
    }

    public override void OnUse()
    {
        base.OnUse();
    
        playerInput.SwitchCurrentActionMap("Fishing");
        rodFishingStartRot = playerItem.TargetRot;
        rodFishingTargetAngle = 0;
        rodFishingTargetX = 0;

        playerItem.SetRotationLock(false);

        UpdateInputUI();
    }

    public override void OnStopUsingItem()
    {
        inputUIDestructor?.Invoke();
        if (hookedFish && hookInRange)
        {
            inventory.AddFish(hookedFish.FishSO);
        }
        base.OnStopUsingItem();
        ResetFishing();
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

    public void OnExitFishing(InputAction.CallbackContext ctx)
    {
        playerInput.SwitchCurrentActionMap("Gameplay");
        playerItem.TargetRot = rodFishingStartRot;
        playerItem.SetRotationLock(true);

        UpdateInputUI();
    }

    public void OnInteract()
    {
        if(hookInRange)
        {
            if (hookedFish)
            {
                inventory.AddFish(hookedFish.FishSO);

                var fishSO = hookedFish.FishSO;

                ResetFishing();
                UpdateInputUI();

                playerItem.EnableTemporaryItem(fishItem);
                (playerItem.EnabledItem as FishItem).SetFish(fishSO);
            }
            else
            {
                ResetFishing();
                UpdateInputUI();
            }
        }
    }

    void Cast()
    {
        hookGO = Instantiate(hookPrefab, rodTipTransform.position, transform.rotation);
        var hook = hookGO.GetComponent<FishingHook>();

        hookGO.GetComponent<Rigidbody>().velocity = rodTipVelocity;
        hook.PlayerFish = this;

        hook.FishHookEvent += OnHookFish;

        var fishingLineGO = Instantiate(fishingLinePrefab);
        fishingLine = fishingLineGO.GetComponent<FishingLine>();

        fishingLine.OnCast(hook, rodTipTransform, rodTipVelocity);

        fishingState = FishingState.Cast;
    }

    void ResetFishing()
    {
        fishingState = FishingState.Uncast;

        hookedFish = null;
        hookInRange = false;

        if (fishingLine)
        {
            Destroy(fishingLine.gameObject);
            fishingLine = null;
        }
        if (hookGO)
        {
            Destroy(hookGO);
            hookGO = null;
        }
    }

    public void SetHookInRange(bool inRange)
    {
        if(hookInRange != inRange)
        {
            hookInRange = inRange;
            UpdateInputUI();
        }
    }

    void UpdateInputUI()
    {
        inputUIDestructor?.Invoke();
        inputUIDestructor = null;

        if (!hookInRange)
        {
            return;
        }
        if (playerInput.currentActionMap.name == parameters.FishingActionMap)
        {
            inputUIDestructor = InputUI.Instance.AddInputUI(exitAction, "Stop fishing");
        }
        else
        {
            if (hookedFish)
            {
                inputUIDestructor = playerInteract.AddInteract(OnInteract, "Collect fish");
            }
            else
            {
                inputUIDestructor = playerInteract.AddInteract(OnInteract, "Reset fishing");
            }
        }
    }

    public void OnHookFish(Fish fish)
    {
        this.hookedFish = fish;
    }
}
