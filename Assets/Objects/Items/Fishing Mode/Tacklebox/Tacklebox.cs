using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tacklebox : MonoBehaviour
{
    [SerializeField] PlayerInventorySO inventory;
    [SerializeField] Transform spawnContainer;

    public FishingModeItem FishingModeItem;

    List<TackleboxItem> items = new();

    void Awake()
    {
        for(int i = 0; i < inventory.TackleboxItems.Count; ++i)
        {
            if(i >= spawnContainer.childCount)
            {
                break;
            }
            Transform spawnPoint = spawnContainer.GetChild(i);
            TackleboxItemSO itemSO = inventory.TackleboxItems[i];
            GameObject go = Instantiate(itemSO.Prefab, spawnPoint.position, Quaternion.identity, transform);
            TackleboxItem item = go.GetComponent<TackleboxItem>();
            items.Add(item);
        }
    }

    void OnEnable()
    {
        UpdateInteractables(FishingModeItem.Phase);
        FishingModeItem.PhaseChangedEvent += UpdateInteractables;
    }

    void OnDisable()
    {
        FishingModeItem.PhaseChangedEvent -= UpdateInteractables;
    }

    void UpdateInteractables(FishingModePhase phase)
    {
        Debug.Log($"PHASE CHANGED: {phase}");
        var value = phase == FishingModePhase.Prepping;
        foreach (var item in items)
        {
            item.enabled = value;
        }
    }
}
