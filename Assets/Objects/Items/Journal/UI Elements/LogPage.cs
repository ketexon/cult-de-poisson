using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogPage : JournalUIElement
{
    [SerializeField] JournalEntrySO entry;
    [SerializeField] CanvasGroup canvasGroup;

    protected override void OnEnable()
    {
        base.Awake();

        canvasGroup.alpha = JournalDataSO.Instance.JournalEntries.Contains(entry) ? 1 : 0;
        JournalDataSO.Instance.NewJournalEntryEvent += OnNewJournalEntry;
    }

    void OnDisable()
    {
        JournalDataSO.Instance.NewJournalEntryEvent -= OnNewJournalEntry;
    }

    void OnNewJournalEntry(JournalEntrySO entry)
    {
        if (entry == this.entry) { 
            canvasGroup.alpha = JournalDataSO.Instance.JournalEntries.Contains(entry) ? 1 : 0;
        }
    }
}
