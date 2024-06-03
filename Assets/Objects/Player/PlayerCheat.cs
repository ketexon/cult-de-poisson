using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCheat : MonoBehaviour
{
    [SerializeField] PlayerInventorySO inventory;
    [SerializeField] InputActionReference cheatAction;
    [SerializeField] List<JournalEntrySO> entries;
    [SerializeField] List<FishSO> journalFish;
    [SerializeField] List<FishSO> inventoryFish;

    bool cheated = false;

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
        cheated = true;
        cheatAction.action.performed -= OnCheat;

        foreach (var entry in entries)
        {
            JournalDataSO.Instance.AddJournalEntry(entry);
        }

        foreach (var f in journalFish)
        {
            JournalDataSO.Instance.AddCaughtFish(f);
        }

        inventory.Clear();
        foreach (var f in inventoryFish)
        {
            inventory.AddFish(f);
        }
    }
}
