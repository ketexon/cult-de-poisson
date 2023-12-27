using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFish : MonoBehaviour
{
    enum FishingState
    {
        Uncast,
        Cast,
        Caught,
    }

    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] PlayerInput input;
    [SerializeField] PlayerItem itemRotate;
    [SerializeField] Transform rodTipTransform;
    [SerializeField] GameObject hookPrefab;
    [SerializeField] float fishingSensitivityY = 0.05f;
    [SerializeField] float fishingSensitivityX = 0.05f;
    [SerializeField] FishingLine fishingLine;

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

    void Reset()
    {
        parameters = FindUtil.Asset<GlobalParametersSO>();
        input = GetComponentInParent<PlayerInput>();
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

    public void OnFish(InputAction.CallbackContext ctx)
    {
        fishing = true;
        cast = false;
        input.SwitchCurrentActionMap("Fishing");
        rodFishingStartRot = itemRotate.TargetRot;
        rodFishingTargetAngle = 0;
        rodFishingTargetX = 0;

        itemRotate.SetRotationLock(false);

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

        itemRotate.TargetRot = rodFishingStartRot * Quaternion.Euler(rodFishingTargetAngle, rodFishingTargetX, 0);
    }

    public void OnFishClick(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if(fishingState == FishingState.Uncast)
            {
                Cast();
            }
            else if(fishingState == FishingState.Cast)
            {

            }
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
        input.SwitchCurrentActionMap("Gameplay");
        itemRotate.TargetRot = rodFishingStartRot;
        itemRotate.SetRotationLock(true);

        UpdateInputUI();
    }

    public void OnUse(InputAction.CallbackContext ctx)
    {
        if(hookedFish && fishInRange)
        {

        }
    }

    void Cast()
    {
        var hookGO = Instantiate(hookPrefab, rodTipTransform.position, transform.rotation);
        var hook = hookGO.GetComponent<FishingHook>();

        hookGO.GetComponent<Rigidbody>().velocity = rodTipVelocity;
        hook.PlayerFish = this;

        hook.FishHookEvent += OnHookFish;

        fishingLine.OnCast(hook, rodTipVelocity);

        fishingState = FishingState.Cast;
    }

    public void SetFishInRange(bool inRange)
    {
        fishInRange = inRange;
        if(inputUIDestructor != null)
        {
            inputUIDestructor.Invoke();
            inputUIDestructor = null;
        }
        if (!fishInRange)
        {
            return;
        }

        if(input.currentActionMap.name == parameters.FishingActionMap)
        {
            inputUIDestructor = InputUI.Instance.AddInputUI(input.actions["Exit"], "Stop fishing");
        }
        else
        {
            inputUIDestructor = InputUI.Instance.AddInputUI(input.actions["Use"], "Collect fish");
        }
    }

    void UpdateInputUI()
    {
        if (!fishInRange)
        {
            return;
        }
        if (inputUIDestructor != null)
        {
            inputUIDestructor.Invoke();
            inputUIDestructor = null;
        }
        if (input.currentActionMap.name == parameters.FishingActionMap)
        {
            inputUIDestructor = InputUI.Instance.AddInputUI(input.actions["Exit"], "Stop fishing");
        }
        else
        {
            inputUIDestructor = InputUI.Instance.AddInputUI(input.actions["Use"], "Collect fish");
        }
    }

    public void OnHookFish(Fish fish)
    {
        this.hookedFish = fish;
    }
}
