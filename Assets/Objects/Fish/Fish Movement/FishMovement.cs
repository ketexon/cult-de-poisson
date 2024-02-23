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
        hookedFish.UnhookEvent += Unhook;
    }

    virtual protected void OnDestroy()
    {
        hookedFish.UnhookEvent -= Unhook;
    }

    virtual public void Unhook()
    {
        enabled = true;
        hookedFish.enabled = false;
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
