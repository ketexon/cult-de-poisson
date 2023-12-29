using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishItem : Item
{
    FishSO fishSO;

    public void SetFish(FishSO fishSO)
    {
        this.fishSO = fishSO;
    }
}
