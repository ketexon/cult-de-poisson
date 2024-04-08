using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(ConfigurableJoint))]
public class FishingHook : MonoBehaviour
{
    public System.Action<bool> VisibilityChangedEvent;
    public System.Action<Fish> OnHook;

    bool _visible = true;
    public bool Visible
    {
        get => _visible;
        set
        {
            if(value != Visible)
            {
                _visible = value;
                VisibilityChangedEvent?.Invoke(value);
            }
        }
    }

    [System.NonSerialized]
    public FishingRod FishingRod;

    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] float waterDrag = 5.0f;
    [SerializeField] float bobDistance = 5.0f;

    public System.Action<Vector3> WaterHitEvent;
    public System.Action<Fish> FishHookEvent;
    public Vector3? WaterHitPos { get; private set; }

    Fish fish = null;
    public Rigidbody RigidBody { get; private set; }
    ConfigurableJoint joint;

    bool inWater = false;

    public float BobDistance => bobDistance;

    float initialDrag;

    public void OnCatchFish(FishSO fish)
    {
        var fishGO = Instantiate(fish.InWaterPrefab, transform);
        this.fish = fishGO.GetComponent<Fish>();
        FishHookEvent?.Invoke(this.fish);
    }

    void Reset()
    {
        parameters = FindUtil.Asset<GlobalParametersSO>();
    }

    void Awake()
    {
        RigidBody = GetComponent<Rigidbody>();
        joint = RigidBody.GetComponent<ConfigurableJoint>();

        initialDrag = RigidBody.drag;

        joint.linearLimit = new SoftJointLimit() {
            limit = bobDistance
        };

        // dont move with parent
        transform.SetParent(null, true);
    }

    void FixedUpdate()
    {
        if (fish && inWater)
        {
            RigidBody.AddForce(fish.ResistanceAcceleration(), ForceMode.Acceleration);            
        }
    }

    public void Reel(float limit)
    {
        joint.linearLimit = new SoftJointLimit()
        {
            limit = limit
        };

        if(limit < parameters.HookDistancePickupRange)
        {
            FishingRod.SetHookInRange(true);
        }
        else if(limit > parameters.HookDistancePickupRange)
        {
            FishingRod.SetHookInRange(false);
        }
    }

    public void AttachToRB(Rigidbody rb)
    {
        joint.connectedBody = rb;
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;
    }

    void OnTriggerEnter(Collider other)
    {
        int otherLayerField = 1 << other.gameObject.layer;
        if ((otherLayerField & parameters.WaterLayerMask.value) > 0)
        {
            WaterHitPos = transform.position;
            WaterHitEvent?.Invoke(WaterHitPos.Value);
            RigidBody.drag = waterDrag;
            inWater = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (parameters.WaterLayerMask.Contains(other.gameObject.layer))
        {
            inWater = false;
            RigidBody.drag = initialDrag;  
        }
    }
}
