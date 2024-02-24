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

    /// <summary>
    /// If we collide with a hook,
    /// call its OnHook callback and 
    /// enable the HookedFish behavior.
    /// Only works if Hook is visible
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter(Collision collision)
    {
        if (!enabled) return;
        if (parameters.HookLayerMask.Contains(collision.gameObject.layer))
        {
            var hook = collision.gameObject.GetComponent<FishingHookV2>();
            if (!hook.Visible) return;
            hook.OnHook?.Invoke(GetComponent<Fish>());
            enabled = false;
            hookedFish.enabled = true;
        }
    }
}
