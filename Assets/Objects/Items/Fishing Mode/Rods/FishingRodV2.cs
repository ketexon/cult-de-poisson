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
public class FishingRodV2 : Interactable
{
    [SerializeField] protected InputActionReference tugAction;
    [SerializeField] protected InputActionReference reelAction;
    [SerializeField] protected InputActionReference interactAction;
    [SerializeField] FishingHookV2 hook;
    [SerializeField] float reelSensitivity = 0.1f;
    [SerializeField] float maxReelPerSecond = 1.0f;

    FishingModeItem _fishingModeItem;
    public FishingModeItem FishingModeItem {
        get => _fishingModeItem;
        set
        {
            // if we are changing the value
            if(_fishingModeItem != value)
            {
                // deregister callbacks on old value
                if (_fishingModeItem)
                {
                    _fishingModeItem.PhaseChangedEvent -= PhaseChangedEvent;
                }
                _fishingModeItem = value;
                // register callbacks on new value
                if (_fishingModeItem)
                {
                    _fishingModeItem.PhaseChangedEvent += PhaseChangedEvent;
                }
            }
        }
    }
    [System.NonSerialized] public float CollectableLineLength;

    public override string InteractMessage => "Start fishing";

    float reeledThisUpdate = 0;

    public FishingHookV2 Hook => hook;

    virtual public bool CanExitFishingMode => true;


    override protected void Awake()
    {
        base.Awake();

        reelAction.action.performed += OnInputReel;
        //interactAction.action.performed += OnInputInteract;
        collider.enabled = false;
    }

    // we define onenable here
    // to prevent default behavior of enabling interaction
    // on enable
    protected override void OnEnable()
    {}


    // we define onenable here
    // to prevent default behavior of disabling interaction
    // on disable
    protected override void OnDisable()
    { }

    virtual protected void OnDestroy()
    {
        reelAction.action.performed -= OnInputReel;
        if (FishingModeItem)
        {
            FishingModeItem.PhaseChangedEvent -= PhaseChangedEvent;
        }
    }


    void PhaseChangedEvent(FishingModePhase newPhase)
    {
        // only allow interacting with fishing rod if
        // we are in the prep phase
        collider.enabled = newPhase == FishingModePhase.Prepping;
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

    virtual protected void Reel(float amount)
    {}

    virtual public void ResetFishing()
    {}
}
