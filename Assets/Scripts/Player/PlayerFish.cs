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

    [SerializeField] PlayerInput input;
    [SerializeField] Transform rodTransform;
    [SerializeField] Transform rodTipTransform;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] GameObject hookPrefab;
    [SerializeField] float rodRotateSpeed = 5;
    [SerializeField] float fishingSensitivityY = 0.05f;
    [SerializeField] float fishingSensitivityX = 0.05f;
    [SerializeField] FishingLine fishingLine;

    FishingState fishingState = FishingState.Uncast;

    bool fishing = false;
    bool cast = false;

    Vector3 rodOffsetPos;
    Quaternion rodCurRot;
    Quaternion rodTargetRot;
    Quaternion rodFishingStartRot;

    Vector3 lastRodTipPos;
    Vector3 rodTipVelocity;

    float rodFishingTargetAngle;
    float rodFishingTargetX;

    float reelStrength = 0;

    void Reset()
    {
        input = GetComponentInParent<PlayerInput>();
        playerMovement = GetComponentInParent<PlayerMovement>();
        rodTransform = this.Query<Transform>().InChildren.NameContains("rod", insensitive: true).Execute();
    }

    void Awake()
    {
        rodCurRot = rodTransform.rotation;
        lastRodTipPos = rodTipTransform.position;
        rodOffsetPos = rodTransform.position - transform.position;
    }

    void Update()
    {
        if (!fishing)
        {
            rodTargetRot = transform.rotation;
        }
        rodCurRot = Quaternion.Lerp(rodCurRot, rodTargetRot, Time.deltaTime * rodRotateSpeed);
        rodTransform.rotation = rodCurRot;

        var rodTipPos = rodTipTransform.position;
        rodTipVelocity = (rodTipPos - lastRodTipPos) / Time.deltaTime;
        lastRodTipPos = rodTipPos;

        rodTransform.position = transform.position + rodOffsetPos;
    }

    public void OnFish(InputAction.CallbackContext ctx)
    {
        fishing = true;
        cast = false;
        input.SwitchCurrentActionMap("Fishing");
        rodFishingStartRot = rodTargetRot;
        rodFishingTargetAngle = 0;
        rodFishingTargetX = 0;
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

        rodTargetRot = rodFishingStartRot * Quaternion.Euler(rodFishingTargetAngle, rodFishingTargetX, 0);
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
        reelStrength = Mathf.Clamp01(-ctx.ReadValue<float>() / 120);
    }

    public void OnExitFishing(InputAction.CallbackContext ctx)
    {
        fishing = false;
        input.SwitchCurrentActionMap("Gameplay");
        rodTargetRot = rodFishingStartRot;
    }

    void Cast()
    {
        var hookGO = Instantiate(hookPrefab, rodTipTransform.position, transform.rotation);
        var hook = hookGO.GetComponent<FishingHook>();

        hookGO.GetComponent<Rigidbody>().velocity = rodTipVelocity;
        hook.PlayerFish = this;

        hook.FishCatchEvent += OnCatchFish;

        fishingLine.SetHook(hook);

        fishingState = FishingState.Cast;
    }

    public void OnCatchFish(Fish fish)
    {

    }
}
