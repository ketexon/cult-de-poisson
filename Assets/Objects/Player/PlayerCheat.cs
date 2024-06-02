using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCheat : MonoBehaviour
{
    [SerializeField] InputActionReference cheatAction;
    [SerializeField] List<JournalEntrySO> entries;
    [SerializeField] List<FishSO> fish;

    void Awake()
    {
        cheatAction.action.performed += OnCheat;
    }

    void OnDestroy()
    {
        cheatAction.action.performed -= OnCheat;
    }

    void OnCheat(InputAction.CallbackContext ctx)
    {
        Debug.Log("Cheater >:(");

        foreach(var entry in entries)
        {
            JournalDataSO.Instance.AddJournalEntry(entry);
        }

        foreach (var f in fish)
        {
            JournalDataSO.Instance.AddCaughtFish(f);
        }
    }
}
