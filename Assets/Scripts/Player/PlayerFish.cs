using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFish : MonoBehaviour
{
    [SerializeField] PlayerInput input;
    [SerializeField] Transform rodTransform;
    [SerializeField] Transform rodTipTransform;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] GameObject hookPrefab;
    [SerializeField] float rodRotateSpeed = 5;
    [SerializeField] float fishingSensitivityY = 0.05f;
    [SerializeField] float fishingSensitivityX = 0.05f;
    [SerializeField] FishingLine fishingLine;

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
            Cast();
        }
    }

    public void OnExitFishing(InputAction.CallbackContext ctx)
    {
        fishing = false;
        input.SwitchCurrentActionMap("Gameplay");
        rodTargetRot = rodFishingStartRot;
    }

    void Cast()
    {
        var hook = Instantiate(hookPrefab, rodTipTransform.position, transform.rotation);
        hook.GetComponent<Rigidbody>().velocity = rodTipVelocity;
        fishingLine.SetHook(hook.GetComponent<FishingHook>());
    }
}
