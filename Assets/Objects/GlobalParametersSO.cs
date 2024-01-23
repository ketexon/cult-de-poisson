using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalParameters", menuName = "Global Parameters")]
public class GlobalParametersSO : ScriptableObject
{
    [Header("Layers")]
    [SerializeField]
    public LayerMask WaterLayerMask;

    [SerializeField]
    public LayerMask HookLayerMask;

    [SerializeField]
    public LayerMask GroundLayerMask;

    [SerializeField]
    public LayerMask InteractLayerMask;

    [SerializeField]
    public LayerMask BucketFishLayerMask;

    [Header("Fish Zones")]
    [Tooltip("How long a hook needs to be in a Fish Zone for a fish to be caught.")]
    [Min(0)]
    [SerializeField] 
    public float MinHookInFishZoneDelay = 5.0f;

    [Tooltip("Max amount of time a hook can be in a Fish Zone until a fish is caught.")]
    [Min(0)]
    [SerializeField] 
    public float MaxHookInFishZoneDelay = 10.0f;

    [Header("Fishing")]
    [SerializeField]
    public string FishingActionMap = "Fishing";

    [SerializeField]
    public float ReelStrength = 5.0f;

    [SerializeField]
    public float HookDistancePickupRange = 2.0f;

    [Header("Interaction")]
    [SerializeField]
    public float InteractDistance = 4.0f;

    public float GetRandomFishZoneDelay()
    {
        return Random.Range(MinHookInFishZoneDelay, MaxHookInFishZoneDelay);
    }

    void Reset()
    {
        HookLayerMask = FindUtil.Layer("hook");
        BucketFishLayerMask = FindUtil.Layer("BucketFish");
    }
}
