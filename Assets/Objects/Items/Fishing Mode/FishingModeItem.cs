using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The logical phase of fishing.
/// See <see cref="Inactive"/>, <see cref="Prepping"/>, and <see cref="Fishing"/>
/// </summary>
public enum FishingModePhase
{
    /// <summary>
    /// The phase where the player is not using the fishing mode
    /// So they cannot use any fishing-related items and cannot
    /// start fishing (ie. interaction is disabled)
    /// </summary>
    Inactive,
    /// <summary>
    /// The phase where the player can use all the fishing mode items
    /// and cannot move.
    /// </summary>
    Prepping,
    /// <summary>
    /// The phase where the player is fishing, so the player cannot 
    /// use any interactables and input is controlled by the rod
    /// </summary>
    Fishing,
};

public class FishingModeItem : Item
{
    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] PlayerInventorySO playerInventory;

    [SerializeField] InputActionReference interactAction;
    [SerializeField] InputActionReference navigateAction;
    [SerializeField] InputActionReference lookAction;
    [SerializeField] InputActionReference exitAction;

    [SerializeField] float fishCollectableLineLength;

    FishingModePhase phase = FishingModePhase.Inactive;
    public FishingModePhase Phase
    {
        get => phase;
        set
        {
            if(value != phase)
            {
                phase = value;
                PhaseChangedEvent?.Invoke(phase);
            }
        }
    }

    public System.Action<FishingModePhase> PhaseChangedEvent;

    // expose so that FishingRod can use it
    public GameObject Player => player;
    public PlayerInteract PlayerInteract => playerInteract;

    static FishingRodSO lastUsedFishingRod = null;

    FishingRodSO fishingRodSO;
    FishingRodV2 fishingRod;

    PlayerMovement playerMovement;


    void Reset()
    {
        parameters = FindObjectOfType<GlobalParametersSO>();
        playerInventory = FindObjectOfType<PlayerInventorySO>();
    }

    public override void Initialize(InitializeParams initParams)
    {
        base.Initialize(initParams);

        if (lastUsedFishingRod)
        {
            fishingRodSO = lastUsedFishingRod;
        }
        else
        {
            fishingRodSO = playerInventory.Rods.Count > 0 ? playerInventory.Rods[0] : null;
        }

        playerMovement = player.GetComponent<PlayerMovement>();

        exitAction.action.performed += OnExit;

        SpawnRod();
    }

    public override void OnStopUsingItem()
    {
        exitAction.action.performed -= OnExit;

        base.OnStopUsingItem();
    }

    public override void OnUse()
    {
        Phase = FishingModePhase.Prepping;

        // switch input
        playerInput.SwitchCurrentActionMap("FishingV2");

        // register look action on playerMovement, so that we can still move the camera
        lookAction.action.performed += playerMovement.OnLook;

        // register interact action on playerInteract, so we can still interact with tackelboxitems
        interactAction.action.performed += playerInteract.OnInteract;

        // confine camera
        playerMovement.ConfineRelative(yawRange: new(-90, 90));

        // make it so the item does not move with the camera
        playerItem.SetRotationLock(false);
    }

    void StopUsing()
    {
        // we can only stop using if we are not fishing nor
        // already inactive
        if (Phase != FishingModePhase.Prepping) return;

        Phase = FishingModePhase.Inactive;
        if (playerInput.currentActionMap.name == "FishingV2")
        {
            lookAction.action.performed -= playerMovement.OnLook;
            
            playerItem.SetRotationLock(true);

            // confine camera
            playerMovement.Unconfine();
            
            playerInput.SwitchCurrentActionMap("Gameplay");
        }
    }

    void OnExit(InputAction.CallbackContext ctx)
    {
        if (!fishingRod.CanExitFishingMode)
        {
            return;
        }

        if (ctx.performed)
        {
            StopUsing();
        }
    }

    void SpawnRod()
    {
        if (!fishingRodSO) return;

        var fishingRodGO = Instantiate(fishingRodSO.Prefab.gameObject, transform);
        fishingRod = fishingRodGO.GetComponent<FishingRodV2>();
        fishingRod.FishingModeItem = this;
        fishingRod.CollectableLineLength = fishCollectableLineLength;
    }

    public void CollectFish(Fish fish)
    {
        var fishSO = fish.FishSO;
        playerInventory.AddFish(fishSO);

        Destroy(fish.gameObject);
        fishingRod.ResetFishing();
    }
}