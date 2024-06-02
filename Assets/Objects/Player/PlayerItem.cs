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
    [SerializeField] InputActionReference cycleItemAction;

    public float RotateSpeed => rotateSpeed;

    [System.NonSerialized] public Vector3 ItemOffsetPos;
    [System.NonSerialized] public Quaternion CurRot;
    [System.NonSerialized] public Quaternion TargetRot;
    [System.NonSerialized] public Quaternion FishingStartRot;


    [SerializeField] SaveStateSO saveState;

    List<Item> items;
    HashSet<Item> heldItems;

    public System.Action<Item> ItemChangeEvent;

    Player player;

    /// <summary>
    /// Whether the item should lock its rotation to the player's rotation.
    /// </summary>
    bool playerLock = true;

    /// <summary>
    /// Parameters we pass to item so that it doesn't have to
    /// find components and GameObjects in its Awake.
    /// </summary>
    Item.InitializeParams itemInitializeParams;

    public int EnabledItemIndex => EnabledItem.transform.GetSiblingIndex();
    public Item EnabledItem { get; protected set; }

    int lastItemIndex;

    public bool IsTemporaryItem { get; protected set; }

    System.Action inputUIDestructor = null;


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
        player = GetComponent<Player>();

        items = new(itemContainerTransform.GetComponentsInChildren<Item>(includeInactive: true));
        heldItems = new(startingItems);

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

        foreach (var item in items)
        {
            item.Initialize(itemInitializeParams);
        }

        EnableItem(startingItemIndex);
    }

    /// <summary>
    /// Adds an item to the player's held items
    /// </summary>
    /// <returns>True if the item was added, false if it was already held</returns>
    public bool AddItem(ItemSO itemSO, bool thenSwitch = true)
    {
        var item = items.Find(i => i.ItemSO == itemSO);
        if (heldItems.Contains(item)) return false;
        heldItems.Add(item);
        if (thenSwitch)
        {
            EnableItem(item);
        }
        return true;
    }

    /// <summary>
    /// Called when scrolling/pressing Q/E.
    /// If we are not holding a temporary item, swaps to the next item.
    /// If we are holding a temporary item, go to the last item used.
    /// </summary>
    /// <param name="ctx"></param>
    public void OnCycleItem(InputAction.CallbackContext ctx)
    {
        if(!ctx.performed || !EnabledItem.CanSwitchItems)
        {
            return;
        }

        float v = ctx.ReadValue<float>();
        int dir = Math.Sign(v);
        CycleItem(dir);
    }

    public void CycleItem(int dir = 1)
    {
        int newItemIndex;

        if (IsTemporaryItem)
        {
            newItemIndex = lastItemIndex;
        }
        else
        {
            newItemIndex = EnabledItemIndex;
            Item item;

            // try cycling until we reach an item held in hand
            do
            {
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
    /// Given an ItemSO, give the index in <see cref="items"/>. If the item does not exist, returns null.
    /// </summary>
    /// <param name="itemSO"></param>
    /// <param name="allowNonHeldItems"></param>
    /// <returns></returns>
    public int? GetItemIndex(ItemSO itemSO, bool allowNonHeldItems = false)
    {
        var index = items.FindIndex(item => {
            return item.ItemSO == itemSO;
        });
        // return the index if the index yielded a result
        // and, if the item is not temporary, it is in the held items
        return index >= 0 && (allowNonHeldItems || heldItems.Contains(items[index])) ? index : null;
    }

    /// <summary>
    /// Given an ItemSO, gives the Item. If the item does not exist, returns null.
    /// </summary>
    /// <param name="itemSO"></param>
    /// <param name="allowNonHeldItems">If true, allows items not currently in the hand to be returned</param>
    /// <returns></returns>
    public Item GetItem(ItemSO itemSO, bool allowNonHeldItems = false)
    {
        return GetItemIndex(itemSO, allowNonHeldItems: allowNonHeldItems) is int idx
            ? items[idx]
            : null;
    }

    /// <summary>
    /// Enables an item by index in inventory.
    /// </summary>
    /// <param name="index"></param>
    public Item EnableItem(int index, bool temporary = false)
    {
        return EnableItem(items[index], temporary);
    }

    /// <summary>
    /// Internal EnableItem to set the currently enabled item and its index.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="index"></param>
    public Item EnableItem(Item item, bool temporary = false)
    {
        if (!temporary)
        {
            if (!heldItems.Contains(item))
            {
                Debug.LogWarning($"Tried to switch to an item not in the inventory: {item}.");
                return null;
            }
        }
        IsTemporaryItem = temporary;
        return EnableItemInternal(item);
    }

    /// <summary>
    /// Enable an item from its ItemSO.
    /// </summary>
    /// <param name="item"></param>
    public Item EnableItem(ItemSO itemSO, bool temporary = false)
    {
        int? itemIndex = GetItemIndex(itemSO, temporary);
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

    /// <summary>
    /// Internal enable item function to actually destroy the old item and instantiate the new item.
    /// </summary>
    /// <param name="item"></param>
    Item EnableItemInternal(Item item)
    {
        if (EnabledItem)
        {
            lastItemIndex = EnabledItemIndex;
            EnabledItem.gameObject.SetActive(false);
        }

        EnabledItem = item;
        item.gameObject.SetActive(true);

        ItemChangeEvent?.Invoke(EnabledItem);

        if (InputUI.Instance)
        {
            RecreateInputUI();
        }

        return item;
    }

    /// <summary>
    /// Rotates the item using lerp.
    /// </summary>
    void Update()
    {
        if (playerLock)
        {
            TargetRot = Quaternion.Euler(0, player.Movement.Yaw, 0);
        }
        CurRot = Quaternion.Lerp(CurRot, TargetRot, Time.deltaTime * rotateSpeed);
        itemContainerTransform.rotation = CurRot;

        if(inputUIDestructor == null && cycleItemAction.action.enabled)
        {
            RecreateInputUI();
        }
        else if(inputUIDestructor != null && !cycleItemAction.action.enabled)
        {
            inputUIDestructor?.Invoke();
            inputUIDestructor = null;
        }
    }

    void RecreateInputUI()
    {
        inputUIDestructor?.Invoke();

        if (EnabledItem && !EnabledItem.CanSwitchItems)
        {
            inputUIDestructor = InputUI.Instance.AddInputUI(
                cycleItemAction.action,
                EnabledItem.SwitchItemsDisabledReason == null ? "to switch items" : $"to switch items ({EnabledItem.SwitchItemsDisabledReason})",
                order: -1000,
                disabled: true
            );
        }
        else
        {
            inputUIDestructor = InputUI.Instance.AddInputUI(
                cycleItemAction.action,
                "to switch items",
                order: -1000
            );
        }
    }

    void LateUpdate()
    {
        itemContainerTransform.position = ItemOffsetPos + transform.position;
    }
}
