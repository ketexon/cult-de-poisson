using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishInteractable : Interactable
{
    public PlayerInventorySO inventory;
    public override string InteractMessage => "Pickup Fish";
    public Fish fish;

    // Start is called before the first frame update
    void Start()
    {
        fish = GetComponent<Fish>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnInteract()
    {
        // If the fish is placed on the ground, add it to the inventory
        
        inventory.AddFish(fish.FishSO);
        Destroy(gameObject);
    }
}
