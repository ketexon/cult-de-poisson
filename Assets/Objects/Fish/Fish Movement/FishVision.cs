using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class FishVision : MonoBehaviour
{
    new SphereCollider collider;

    public System.Action<FishingHook> HookVisibleEvent;
    public System.Action HookInvisibleEvent;

    FishingHook hook;

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        UnityEditor.Handles.Label(
            transform.position,
            $"HOOK: {(!hook ? "NONE" : hook.Visible ? "VISIBLE" : "INVISIBLE")}"
        );
    }
#endif

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
        hook = other.GetComponent<FishingHook>();

        hook.VisibilityChangedEvent += OnHookVisibilityChanged;

        if (hook.Visible)
        {
            HookVisibleEvent?.Invoke(hook);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!enabled || !hook) return;

        hook.VisibilityChangedEvent -= OnHookVisibilityChanged;

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
