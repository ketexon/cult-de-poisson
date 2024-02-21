using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Fish))]
public class FishMovement : MonoBehaviour
{
    [SerializeField] protected GlobalParametersSO parameters;

    [System.NonSerialized]
    public FishZone FishZone;

    HookedFish hookedFish;

    virtual protected void Awake()
    {
        hookedFish = GetComponent<HookedFish>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!enabled) return;
        if (parameters.HookLayerMask.Contains(collision.gameObject.layer))
        {
            collision.gameObject.GetComponent<FishingHookV2>().OnHook?.Invoke(GetComponent<Fish>());
            enabled = false;
            hookedFish.enabled = true;
        }
    }
}
