using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class KeyFishItemBehaviour : FishItemBehaviour
{
    [SerializeField] PlayerInventorySO inventory;
    [SerializeField] string doorTag = "Door";

    Fish fish;

    public override bool AgentInteractVisible(Interactable interactable) => interactable.gameObject.CompareTag(doorTag);
    public override string AgentInteractMessage(Interactable _) => "to open door";

    void Awake()
    {
        fish = GetComponent<Fish>();
    }

    public override void OnInteract(Interactable target)
    {
        target.OnInteract();

        inventory.RemoveFish(fish.FishSO);
        Player.Instance.Item.CycleItem();
    }
}