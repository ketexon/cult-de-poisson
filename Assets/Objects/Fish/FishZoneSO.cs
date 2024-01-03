using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FishZone", menuName = "Fish Zone")]
public class FishZoneSO : ScriptableObject
{
    [SerializeField] public List<FishSO> Fish;
}
