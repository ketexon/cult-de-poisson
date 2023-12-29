using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Inventory", menuName = "State/Inventory")]
public class PlayerInventorySO : SavableSO
{
    public override string Key => "inventory";

    [SerializeField] public List<FishSO> Fish = new();
}
