using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerItem : MonoBehaviour
{
    [SerializeField] Transform itemTransform;
    [SerializeField] float rotateSpeed = 5;
    [SerializeField] GameObject defaultItem;

    [System.NonSerialized] public Vector3 ItemOffsetPos;
    [System.NonSerialized] public Quaternion CurRot;
    [System.NonSerialized] public Quaternion TargetRot;
    [System.NonSerialized] public Quaternion FishingStartRot;

    bool playerLock = true;

    int enabledItemIndex;

    public void SetRotationLock(bool value)
    {
        playerLock = value;
    }

    void Awake()
    {
        CurRot = itemTransform.rotation;
        ItemOffsetPos = itemTransform.position - transform.position;

        // enable item
        if (defaultItem != null)
        {
            foreach (Transform t in itemTransform)
            {
                t.gameObject.SetActive(t.gameObject == defaultItem);
            }

            enabledItemIndex = defaultItem.transform.GetSiblingIndex();
        }
        else
        {
            bool enabledItem = false;
            foreach (Transform t in itemTransform)
            {
                if (enabledItem)
                {
                    t.gameObject.SetActive(false);
                }
                else
                {
                    enabledItem = t.gameObject.activeSelf;
                    enabledItemIndex = t.GetSiblingIndex();
                }
            }
            if (!enabledItem && transform.childCount > 0)
            {
                transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    public void OnCycleItem(InputAction.CallbackContext ctx)
    {
        if(!ctx.performed)
        {
            return;
        }
        float v = ctx.ReadValue<float>();
        int newItemIndex = (Math.Sign(v) + enabledItemIndex + itemTransform.childCount) % itemTransform.childCount;

        EnableItem(newItemIndex);
    }

    public void EnableItem(int index)
    {
        itemTransform.GetChild(enabledItemIndex).gameObject.SetActive(false);

        enabledItemIndex = index;
        itemTransform.GetChild(enabledItemIndex).gameObject.SetActive(true);
    }

    public void EnableItem(GameObject item)
    {
        EnableItem(item.transform.GetSiblingIndex());
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
