using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JournalEntryInteractable : Interactable
{
    [SerializeField]
    JournalEntrySO entry;

    public override bool TargetInteractVisible => true;
    public override bool TargetInteractEnabled => true;

    public override string TargetInteractMessage => "to pickup journal scroll";

    public override void OnInteract()
    {
        base.OnInteract();

        JournalDataSO.Instance.AddJournalEntry(entry);

        Destroy(gameObject);
    }
}
