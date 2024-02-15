using UnityEngine;

class FishingModeItem : Item
{
    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] PlayerInventorySO playerInventory;

    FishingRodV2 fishingRod;

    void Reset()
    {
        parameters = FindObjectOfType<GlobalParametersSO>();
        playerInventory = FindObjectOfType<PlayerInventorySO>();
    }

    public override void Initialize(InitializeParams initParams)
    {
        base.Initialize(initParams);
    }

    public override void OnStopUsingItem()
    {
        base.OnStopUsingItem();
    }
}