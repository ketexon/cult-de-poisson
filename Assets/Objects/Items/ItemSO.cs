using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Item")]
public class ItemSO : ScriptableObject
{
    [SerializeField] public string Name;
    [SerializeField] public GameObject Prefab;
}
