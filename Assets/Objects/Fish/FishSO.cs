using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Fish", menuName = "Fish")]
public class FishSO : ScriptableObject
{
    public string Name;
    public GameObject InWaterPrefab;
    public GameObject InHandPrefab;
}
