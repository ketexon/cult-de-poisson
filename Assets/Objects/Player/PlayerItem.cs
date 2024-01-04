using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.VolumeComponent;

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

    bool playerLock = true;

    Item.InitializeParams itemInitializeParams;

    public int EnabledItemIndex { get; protected set; }
    public Item EnabledItem { get; protected set; }

    public bool IsTemporaryItem { get; protected set; }

    void Reset()
    {
        mainCamera = FindObjectOfType<Camera>();
    }

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
        }

        EnableItem(newItemIndex);
    }

    public void OnUseItem(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && EnabledItem)
        {
            EnabledItem.OnUse();
        }
    }

    public void EnableItem(int index)
    {
        EnableItem(items[index], index);
    }

    void EnableItem(ItemSO item, int index)
    {
        IsTemporaryItem = false;
        EnabledItemIndex = index;
        EnableItemInternal(item);
    }

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
