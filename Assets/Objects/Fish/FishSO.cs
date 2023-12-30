using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Fish", menuName = "Fish")]
public class FishSO : ScriptableObject
{
    public string Name;
    public GameObject InWaterPrefab;
    public GameObject InHandPrefab;

    void Reset()
    {
        InHandPrefab = FindUtil.QueryAsset<GameObject>()
            .SiblingTo(this)
            .NameContains("hand", insensitive: true)
            .Execute();

        InWaterPrefab = new FindUtil.FindAssetQuery<GameObject>()
            .SiblingTo(this)
            .NameContains("water", insensitive: true)
            .Execute();
    }
}
