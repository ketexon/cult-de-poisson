using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishItem : Item
{
    FishSO fishSO;
    GameObject fishGO;

    public void SetFish(FishSO fishSO)
    {
        this.fishSO = fishSO;
        if (fishGO)
        {
            Destroy(fishGO);
        }
        fishGO = Instantiate(fishSO.InHandPrefab, this.transform);
    }
}
