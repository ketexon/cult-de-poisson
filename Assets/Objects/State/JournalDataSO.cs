using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Journal Data", menuName = "State/Journal Data")]
public class JournalDataSO : SingletonObject<JournalDataSO>
{
    HashSet<FishSO> caughtFish = new();
    HashSet<JournalEntrySO> journalEntries = new();
    
    public HashSet<FishSO> CaughtFish => caughtFish;
    public System.Action<FishSO> NewFishCaughtEvent;

    public HashSet<JournalEntrySO> JournalEntries => journalEntries;
    public System.Action<JournalEntrySO> NewJournalEntryEvent;

    public void AddCaughtFish(FishSO f)
    {
        if (!caughtFish.Contains(f))
        {
            caughtFish.Add(f);
            NewFishCaughtEvent?.Invoke(f);
        }
    }

    public void AddJournalEntry(JournalEntrySO e)
    {
        if (!journalEntries.Contains(e))
        {
            journalEntries.Add(e);
            NewJournalEntryEvent?.Invoke(e);
        }
    }
}
