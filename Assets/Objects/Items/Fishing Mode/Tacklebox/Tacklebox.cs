using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tacklebox : MonoBehaviour
{
    [SerializeField] PlayerInventorySO inventory;
    [SerializeField] Transform spawnContainer;

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
        
    }

    void OnDisable()
    {
        
    }
}
