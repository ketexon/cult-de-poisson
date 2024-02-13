using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class FishVision : MonoBehaviour
{
    new SphereCollider collider;

    public System.Action<FishingHook> HookVisibleEvent;
    public System.Action HookInvisibleEvent;

    void Awake()
    {
        collider = GetComponent<SphereCollider>();
    }

    public virtual void SetSize(float size)
    {
        collider.radius = size * 2;
    }

    void OnTriggerEnter(Collider other)
    {
        HookVisibleEvent?.Invoke(other.GetComponent<FishingHook>());
    }

    void OnTriggerExit(Collider other)
    {
        HookInvisibleEvent?.Invoke();
    }
}
