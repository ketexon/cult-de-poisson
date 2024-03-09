using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticFishingToyInteractable : Interactable
{
    [SerializeField] ItemSO item;

    public override void OnInteract()
    {
        base.OnInteract();

        Player.Instance.Item.EnableTemporaryItem(item);
    }
}
