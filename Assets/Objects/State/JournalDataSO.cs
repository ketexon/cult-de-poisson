using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Journal Data", menuName = "State/Journal Data")]
public class JournalDataSO : ScriptableObject
{
    HashSet<FishSO> caughtFish = new();
    
    public IReadOnlyCollection<FishSO> CaughtFish => caughtFish;
    public System.Action<FishSO> NewFishCaughtEvent;

    public void AddCaughtFish(FishSO f)
    {
        if (!caughtFish.Contains(f))
        {
            caughtFish.Add(f);
            NewFishCaughtEvent?.Invoke(f);
        }
    }
}
