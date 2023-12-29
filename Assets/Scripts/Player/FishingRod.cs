using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [SerializeField] InputActionReference interactAction;
    [SerializeField] InputActionReference moveAction;
    [SerializeField] InputActionReference clickAction;
    [SerializeField] InputActionReference reelAction;
    [SerializeField] InputActionReference exitAction;

    GameObject hookGO;
    FishingLine fishingLine;

    FishingState fishingState = FishingState.Uncast;

    bool fishing = false;
    bool cast = false;

    Quaternion rodFishingStartRot;

    Vector3 lastRodTipPos;
    Vector3 rodTipVelocity;

    float rodFishingTargetAngle;
    float rodFishingTargetX;

    float reelStrength = 0;

    bool fishInRange = false;

    Fish hookedFish;

    System.Action inputUIDestructor = null;

    public override void Initialize(GameObject player, PlayerInput playerInput, PlayerItem playerItem)
    {
        base.Initialize(player, playerInput, playerItem);

        interactAction.action.performed += OnInteract;

        moveAction.action.performed += OnFishMove;
        moveAction.action.canceled += OnFishMove;

        clickAction.action.performed += OnFishClick;
        reelAction.action.performed += OnFishReel;
        exitAction.action.performed += OnExitFishing;
    }

    void Reset()
    {
        parameters = FindUtil.Asset<GlobalParametersSO>();
        playerInput = GetComponentInParent<PlayerInput>();
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
    
        fishing = true;
        cast = false;
        playerInput.SwitchCurrentActionMap("Fishing");
        rodFishingStartRot = playerItem.TargetRot;
        rodFishingTargetAngle = 0;
        rodFishingTargetX = 0;

        playerItem.SetRotationLock(false);

        UpdateInputUI();
    }

    public override void OnStopUsingItem()
    {
        base.OnStopUsingItem();
        Destroy(fishingLine.gameObject);
        Destroy(hookGO);
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
        else if(fishingState == FishingState.Cast)
        {

        }
    }

    public void OnFishReel(InputAction.CallbackContext ctx)
    {
        reelStrength = Mathf.Clamp(-ctx.ReadValue<float>() / 120, -1, 1);
        fishingLine.Reel(reelStrength);
    }

    public void OnExitFishing(InputAction.CallbackContext ctx)
    {
        fishing = false;
        playerInput.SwitchCurrentActionMap("Gameplay");
        playerItem.TargetRot = rodFishingStartRot;
        playerItem.SetRotationLock(true);

        UpdateInputUI();
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if(hookedFish && fishInRange)
        {
            playerItem.EnableTemporaryItem(fishItem);
            (playerItem.EnabledItem as FishItem).SetFish(hookedFish.FishSO);
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

    public void SetFishInRange(bool inRange)
    {
        if(fishInRange != inRange)
        {
            fishInRange = inRange;
            UpdateInputUI();
        }
    }

    void UpdateInputUI()
    {
        if (inputUIDestructor != null)
        {
            inputUIDestructor.Invoke();
            inputUIDestructor = null;
        }
        if (!fishInRange)
        {
            return;
        }
        if (playerInput.currentActionMap.name == parameters.FishingActionMap)
        {
            inputUIDestructor = InputUI.Instance.AddInputUI(exitAction, "Stop fishing");
        }
        else
        {
            inputUIDestructor = InputUI.Instance.AddInputUI(interactAction, "Collect fish");
        }
    }

    public void OnHookFish(Fish fish)
    {
        this.hookedFish = fish;
    }
}
