using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Base class for fishing rods used in <see cref="global::FishingModeItem"/>.
/// Performs input calculations needed in all fishing
/// </summary>
public class FishingRodV2 : MonoBehaviour
{
    [SerializeField] protected InputActionReference tugAction;
    [SerializeField] protected InputActionReference reelAction;
    [SerializeField] protected InputActionReference interactAction;
    [SerializeField] FishingHookV2 hook;
    [SerializeField] float reelSensitivity = 0.1f;
    [SerializeField] float maxReelPerSecond = 1.0f;

    [System.NonSerialized] public FishingModeItem FishingModeItem;
    [System.NonSerialized] public float CollectableLineLength;

    float reeledThisUpdate = 0;

    public FishingHookV2 Hook => hook;

    virtual public bool CanExitFishingMode => true;

    virtual protected void Awake()
    {
        reelAction.action.performed += OnInputReel;
        interactAction.action.performed += OnInputInteract;
    }

    virtual protected void OnDestroy()
    {
        reelAction.action.performed -= OnInputReel;
        interactAction.action.performed -= OnInputInteract;
    }

    virtual protected void Update()
    {
        if(reeledThisUpdate > 0)
        {
            // clamp the amount reeled so we dont exceed maxReelPerSecond
            Reel(Mathf.Min(reeledThisUpdate, maxReelPerSecond * Time.deltaTime));
            reeledThisUpdate = 0;
        }
    }

    virtual public void Cast()
    {}

    void OnInputReel(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        float v = -ctx.ReadValue<float>() / 120;
        if(v > 0)
        {
            // save the amount reeled so we can apply it just once per update
            // this makes it easier to calcualate maximum reel per second
            reeledThisUpdate += v;
        }
    }

    private void OnInputInteract(InputAction.CallbackContext ctx)
    {
        if(!ctx.performed) return;

        Interact();
    }

    virtual protected void Reel(float amount)
    {}

    virtual protected void Interact()
    { }

    virtual public void ResetFishing()
    {}
}
