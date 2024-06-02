using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishInteractable : Interactable
{
    [SerializeField] PlayerInventorySO inventorySO;

    public override string TargetInteractMessage => inventorySO.Full ? $"to pick up {fishSO.Name} (inventory full)" : $"to pick up {fishSO.Name}";
    public override bool TargetInteractEnabled => !inventorySO.Full;

    FishSO fishSO;

    public void Awake()
    {
        fishSO = GetComponent<Fish>().FishSO;

    }
    public override void OnInteract()
    {
        PickUpFish();            
    }

    public void PickUpFish()
    {
        // Add fish to player inventory
        inventorySO.AddFish(fishSO);
        // Destroy fish object
        Destroy(gameObject);
    }
}
