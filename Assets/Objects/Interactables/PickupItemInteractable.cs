using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItemInteractable : Interactable
{
    [SerializeField] ItemSO item;
    [SerializeField] bool destroyAfter = true;

    public override bool TargetInteractVisible => true;
    public override bool TargetInteractEnabled => true;

    public override string TargetInteractMessage => $"to pick up {item.Name}";

    public override void OnInteract()
    {
        Player.Instance.Item.AddItem(item, thenSwitch: true);
        if (destroyAfter)
        {
            Destroy(gameObject);
        }
    }
}
