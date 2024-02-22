using UnityEngine;
using UnityEngine.InputSystem;

class FishingModeItem : Item
{
    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] PlayerInventorySO playerInventory;

    [SerializeField] InputActionReference castAction;
    [SerializeField] InputActionReference navigateAction;
    [SerializeField] InputActionReference lookAction;
    [SerializeField] InputActionReference exitAction;

    static FishingRodSO lastUsedFishingRod = null;

    FishingRodSO fishingRodSO;
    FishingRodV2 fishingRod;

    PlayerMovement playerMovement;

    bool usingRod = false;

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
        castAction.action.performed += OnCast;

        SpawnRod();
    }

    public override void OnStopUsingItem()
    {
        exitAction.action.performed -= OnExit;
        castAction.action.performed -= OnCast;

        base.OnStopUsingItem();
    }

    public override void OnUse()
    {
        usingRod = true;

        // switch input
        playerInput.SwitchCurrentActionMap("FishingV2");

        // register look action on playerMovement, so that we can still move the camera
        lookAction.action.performed += playerMovement.OnLook;

        // confine camera
        playerMovement.ConfineRelative(yawRange: new(-90, 90));

        // make it so the item does not move with the camera
        playerItem.SetRotationLock(false);
    }

    void StopUsing()
    {
        if (!usingRod) return;

        usingRod = false;
        if(playerInput.currentActionMap.name == "FishingV2")
        {
            lookAction.action.performed -= playerMovement.OnLook;
            
            playerItem.SetRotationLock(true);

            // confine camera
            playerMovement.Unconfine();
            
            playerInput.SwitchCurrentActionMap("Gameplay");
        }
    }

    void OnCast(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            fishingRod.Cast();
        }
    }

    void OnExit(InputAction.CallbackContext ctx)
    {
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
    }
}