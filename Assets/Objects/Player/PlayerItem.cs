using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
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
    [SerializeField] Transform itemTransform;
    [SerializeField] float rotateSpeed = 5;
    [SerializeField] List<ItemSO> startingItems;
    [SerializeField] int startingItemIndex;

    [System.NonSerialized] public Vector3 ItemOffsetPos;
    [System.NonSerialized] public Quaternion CurRot;
    [System.NonSerialized] public Quaternion TargetRot;
    [System.NonSerialized] public Quaternion FishingStartRot;

    [SerializeField] SaveStateSO saveState;

    List<ItemSO> items;

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
        items = startingItems;
        EnabledItemIndex = startingItemIndex;

        foreach(Transform t in itemTransform)
        {
            Destroy(t.gameObject);
        }

        CurRot = itemTransform.rotation;
        ItemOffsetPos = itemTransform.position - transform.position;

        itemInitializeParams = new()
        {
            Player = gameObject,
            PlayerInput = playerInput,
            PlayerItem = this,
            PlayerInteract = playerInteract,
            MainCamera = mainCamera,
            CinemachineBrain = cinemachineBrain,
        };

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
            newItemIndex = (Math.Sign(v) + EnabledItemIndex + items.Count) % items.Count;
            Debug.Log(newItemIndex);
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
    /// Enables an item by index in inventory.
    /// </summary>
    /// <param name="index"></param>
    public void EnableItem(int index)
    {
        EnableItem(items[index], index);
    }

    /// <summary>
    /// Internal EnableItem to set the currently enabled item and its index.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="index"></param>
    void EnableItem(ItemSO item, int index)
    {
        IsTemporaryItem = false;
        EnabledItemIndex = index;
        EnableItemInternal(item);
    }

    /// <summary>
    /// Enable an item from its ItemSO.
    /// </summary>
    /// <param name="item"></param>
    public void EnableItem(ItemSO item)
    {
        // if no index supplied, try to determine it
        var index = items.FindIndex((otherItem) => otherItem == item);
        Debug.Assert(
            index >= 0,
            $"Could not find item \"{item}\" in currently held items.\n"
            + "Use EnableTemporaryItem to add an item not in held items."
        );

        EnableItem(item, index);
    }

    // This enables the item but makes it so when you cycle items, it goes
    // back to the last item used, since they have no index.
    public void EnableTemporaryItem(ItemSO item)
    {
        IsTemporaryItem = true;
        EnableItemInternal(item);
    }

    /// <summary>
    /// Internal enable item function to actually destroy the old item and instantiate the new item.
    /// </summary>
    /// <param name="item"></param>
    void EnableItemInternal(ItemSO item)
    {
        if (EnabledItem)
        {
            EnabledItem.OnStopUsingItem();
        }

        var itemGO = Instantiate(item.Prefab, itemTransform);
        EnabledItem = itemGO.GetComponent<Item>();
        EnabledItem.Initialize(itemInitializeParams);

        ItemChangeEvent?.Invoke(EnabledItem);
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
        itemTransform.rotation = CurRot;

        itemTransform.position = ItemOffsetPos + transform.position;
    }
}
