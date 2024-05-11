using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishInteractable : Interactable
{
    [SerializeField] string InteractMessageTemplate = "Pick up {0}";
    [SerializeField] PlayerInventorySO inventorySO;
    private string fishName;
    FishSO fishSO;

    public void Awake()
    {
        Fish temp = GetComponent<Fish>();

        fishSO = temp.FishSO;
        fishName = fishSO.Name;

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

    public override string TargetInteractMessage => string.Format(InteractMessageTemplate, fishName);
}
