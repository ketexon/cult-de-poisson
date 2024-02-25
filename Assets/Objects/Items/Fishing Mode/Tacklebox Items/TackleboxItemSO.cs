using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TackleboxItem", menuName = "Fishing/Tacklebox Item")]
public class TackleboxItemSO : ScriptableObject
{
    public string Name;
    public GameObject Prefab;
}
