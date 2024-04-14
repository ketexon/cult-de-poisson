using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using static UnityEngine.Rendering.VolumeComponent;

/// <summary>
/// Manages spawning items, cycling items, showing temporary items, and making the item move with player rotation.
/// Temporary items are items that the player cannot cycle to, but can hold. For example, the player
/// cannot scroll to the fish item, but can activate that item through the bucket.
/// </summary>
public class PlayerItem : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    [SerializeField] PlayerInteract playerInteract;
    [SerializeField] Camera mainCamera;
    [SerializeField] CinemachineBrain cinemachineBrain;
    [FormerlySerializedAs("itemTransform")]
    [SerializeField] Transform itemContainerTransform;
    [SerializeField] float rotateSpeed = 5;
    [SerializeField] List<Item> startingItems;
    [SerializeField] int startingItemIndex;

    [System.NonSerialized] public Vector3 ItemOffsetPos;
    [System.NonSerialized] public Quaternion CurRot;
    [System.NonSerialized] public Quaternion TargetRot;
    [System.NonSerialized] public Quaternion FishingStartRot;

    [SerializeField] SaveStateSO saveState;

    List<Item> items;
    HashSet<Item> heldItems;

    public System.Action<Item> ItemChangeEvent;

    /// <summary>
    /// Whether the item should lock its rotation to the player's rotation.
    /// </summary>
    bool playerLock = true;

    /// <summary>
    /// Parameters we pass to item so that it doesn't have to 
    /// find components and GameObjects in its Awake.
    /// </summary>
    Item.InitializeParams itemInitializeParams;

    public int EnabledItemIndex { get; protected set; }
    public Item EnabledItem { get; protected set; }

    public bool IsTemporaryItem { get; protected set; }

    void Reset()
    {
        mainCamera = FindObjectOfType<Camera>();
    }

    /// <summary>
    /// Stop the PlayerItem from automatically rotating.
    /// Unlocking lets you manually rotate an item.
    /// </summary>
    /// <param name="value"></param>
    public void SetRotationLock(bool value)
    {
        playerLock = value;
    }

    void Awake()
    {
        items = new(itemContainerTransform.GetComponentsInChildren<Item>(includeInactive: true));
        heldItems = new(startingItems);
        EnabledItemIndex = startingItemIndex;

        // disable all items that are not the starting item
        foreach (var item in items)
        {
            item.gameObject.SetActive(false);
        }

        CurRot = itemContainerTransform.rotation;
        ItemOffsetPos = itemContainerTransform.position - transform.position;

        itemInitializeParams = new()
        {
            Player = gameObject,
            PlayerInput = playerInput,
            PlayerItem = this,
            PlayerInteract = playerInteract,
            MainCamera = mainCamera,
            CinemachineBrain = cinemachineBrain,
        };

        foreach (var item in heldItems)
        {
            item.Initialize(itemInitializeParams);
        }

        EnableItem(EnabledItemIndex);
    }

    /// <summary>
    /// Called when scrolling/pressing Q/E. 
    /// If we are not holding a temporary item, swaps to the next item.
    /// If we are holding a temporary item, go to the last item used.
    /// </summary>
    /// <param name="ctx"></param>
    public void OnCycleItem(InputAction.CallbackContext ctx)
    {
        if(!ctx.performed)
        {
            return;
        }
        int newItemIndex;

        if (IsTemporaryItem)
        {
            newItemIndex = EnabledItemIndex;
        }
        else
        {
            float v = ctx.ReadValue<float>();
            int dir = Math.Sign(v);
            newItemIndex = EnabledItemIndex;
            Item item;

            // try cycling until we reach an item held in hand
            do {
                newItemIndex = (newItemIndex + dir + items.Count) % items.Count;
                item = items[newItemIndex];
            } while (!heldItems.Contains(item));
        }

        EnableItem(newItemIndex);
    }

    /// <summary>
    /// When the player presses space bar, relay that to the item.
    /// </summary>
    /// <param name="ctx"></param>
    public void OnUseItem(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && EnabledItem)
        {
            EnabledItem.OnUse();
        }
    }

    /// <summary>
    /// Given an ItemSO, give the index in <see cref="items"/>
    /// </summary>
    /// <param name="itemSO"></param>
    /// <param name="temporary"></param>
    /// <returns></returns>
    int? GetItemIndexFromSO(ItemSO itemSO, bool temporary = false)
    {
        var index = items.FindIndex(item => {
            return item.ItemSO == itemSO;
        });
        // return the index if the index yielded a result
        // and, if the item is not temporary, it is in the held items
        return index >= 0 && (temporary || heldItems.Contains(items[index])) ? index : null;
    }

    /// <summary>
    /// Enables an item by index in inventory.
    /// </summary>
    /// <param name="index"></param>
    public Item EnableItem(int index, bool temporary = false)
    {
        var item = items[index];
        if(!temporary && !heldItems.Contains(item))
        {
            Debug.LogWarning($"Tried to switch to an item not in the inventory: {item}.");
            return null;
        }
        IsTemporaryItem = temporary;
        return EnableItem(item, index, temporary);
    }

    /// <summary>
    /// Internal EnableItem to set the currently enabled item and its index.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="index"></param>
    Item EnableItem(Item item, int index, bool temporary = false)
    {
        if (!temporary)
        {
            EnabledItemIndex = index;
        }
        return EnableItemInternal(item);
    }

    /// <summary>
    /// Enable an item from its ItemSO.
    /// </summary>
    /// <param name="item"></param>
    public Item EnableItem(ItemSO itemSO, bool temporary = false)
    {
        int? itemIndex = GetItemIndexFromSO(itemSO, temporary);
        Debug.Assert(
            itemIndex != null,
            $"Could not find item \"{itemSO}\" in currently held items.\n"
            + "Use EnableTemporaryItem to add an item not in held items."
        );

        if (itemIndex != null)
        {
            return EnableItem(itemIndex.Value, temporary: temporary);
        }
        return null;
    }

    // This enables the item but makes it so when you cycle items, it goes
    // back to the last item used, since they have no index.
    public Item EnableTemporaryItem(Item item)
    {
        IsTemporaryItem = true;
        return EnableItemInternal(item);
    }

    /// <summary>
    /// Internal enable item function to actually destroy the old item and instantiate the new item.
    /// </summary>
    /// <param name="item"></param>
    Item EnableItemInternal(Item item)
    {
        if (EnabledItem)
        {
            EnabledItem.gameObject.SetActive(false);
        }

        EnabledItem = item;
        item.gameObject.SetActive(true);

        ItemChangeEvent?.Invoke(EnabledItem);

        return item;
    }

    /// <summary>
    /// Rotates the item using lerp.
    /// </summary>
    void Update()
    {
        if (playerLock)
        {
            TargetRot = transform.rotation;
        }
        CurRot = Quaternion.Lerp(CurRot, TargetRot, Time.deltaTime * rotateSpeed);
        itemContainerTransform.rotation = CurRot;

        itemContainerTransform.position = ItemOffsetPos + transform.position;
    }
}
