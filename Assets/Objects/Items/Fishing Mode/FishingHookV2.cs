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

    Rigidbody rb;
    float drag;

    Fish fish;

    void Reset()
    {
        parameters = FindObjectOfType<GlobalParametersSO>();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        drag = rb.drag;

        OnHook += OnHookInternal;
    }

    void OnHookInternal(Fish fish)
    {
        this.fish = fish;
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
        if (fish)
        {
            fish.transform.position = transform.position;
        }
    }
}
