using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class FishVision : MonoBehaviour
{
    new SphereCollider collider;

    public System.Action<FishingHookV2> HookVisibleEvent;
    public System.Action HookInvisibleEvent;

    FishingHookV2 hook;

    void Awake()
    {
        collider = GetComponent<SphereCollider>();
    }

    public virtual void SetSize(float size)
    {
        collider.radius = size * 2;
    }

    void OnDisable()
    {
        if (hook)
        {
            hook.VisibilityChangedEvent -= OnHookVisibilityChanged;
            hook = null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!enabled) return;
        hook = other.GetComponent<FishingHookV2>();

        if (hook.Visible)
        {
            HookVisibleEvent?.Invoke(hook);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!enabled || !hook) return;
        HookInvisibleEvent?.Invoke();
        hook = null;
    }

    void OnHookVisibilityChanged(bool newValue)
    {
        if (newValue)
        {
            HookVisibleEvent?.Invoke(hook);
        }
        else
        {
            HookInvisibleEvent?.Invoke();
        }
    }
}
