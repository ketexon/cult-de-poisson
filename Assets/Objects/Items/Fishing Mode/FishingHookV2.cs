using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FishingHookV2 : MonoBehaviour
{
    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] float waterDrag = 5.0f;

    [System.NonSerialized] public BaitSO Bait;

    public System.Action<Fish> OnHook;

    new Collider collider;
    Rigidbody rb;
    float drag;

    Fish fish;
    HookedFish hookedFish;

    void Reset()
    {
        parameters = FindObjectOfType<GlobalParametersSO>();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        drag = rb.drag;

        OnHook += OnHookInternal;
    }

    void OnDestroy()
    {
        if (hookedFish)
        {
            hookedFish.UnhookEvent -= Unhook;
        }
    }

    void OnHookInternal(Fish fish)
    {
        this.fish = fish;
        hookedFish = fish.GetComponent<HookedFish>();

        fish.AttachTo(rb);

        collider.enabled = false;
        // since we disable the collider, we don't get any
        // more OnTrigger_ updates, so we should reset the
        // drag here
        rb.drag = drag;

        hookedFish.UnhookEvent += Unhook;
    }

    void Unhook()
    {
        hookedFish.UnhookEvent -= Unhook;

        fish.Detach();

        ResetHook();
    }

    void OnTriggerEnter(Collider other)
    {
        if (parameters.WaterLayerMask.Contains(other.gameObject.layer))
        {
            rb.drag = waterDrag;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (parameters.WaterLayerMask.Contains(other.gameObject.layer))
        {
            rb.drag = drag;
        }
    }

    void Update()
    {
        //if (fish)
        //{
        //    fish.transform.position = transform.position;
        //}
    }

    public void Break()
    {
        collider.enabled = false;
    }

    public void Fix()
    {
        collider.enabled = true;
    }

    public void ResetHook()
    {
        fish = null;
        hookedFish = null;

        collider.enabled = true;
    }
}
