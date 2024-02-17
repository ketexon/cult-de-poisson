using UnityEngine;
using UnityEngine.InputSystem;

class FishingModeItem : Item
{
    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] PlayerInventorySO playerInventory;

    [SerializeField] InputActionReference navigateAction;
    [SerializeField] InputActionReference exitAction;

    static FishingRodSO lastUsedFishingRod = null;

    FishingRodSO fishingRodSO;
    FishingRodV2 fishingRod;

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
        usingRod = true;
        playerInput.SwitchCurrentActionMap("FishingV2");
    }

    void StopUsing()
    {
        if (!usingRod) return;

        usingRod = false;
        playerInput.SwitchCurrentActionMap("Gameplay");
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