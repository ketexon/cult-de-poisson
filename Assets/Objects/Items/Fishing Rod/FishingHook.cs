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

    [SerializeField] FishingRod fishingRod;

    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] float waterDrag = 5.0f;
    [SerializeField] float bobDistance = 5.0f;

    public System.Action<Vector3> WaterHitEvent;

    public Vector3? WaterHitPos { get; private set; }

    Fish fish = null;
    HookedFish hookedFish = null;

    public Rigidbody RigidBody { get; private set; }

    new Collider collider;
    
    ConfigurableJoint joint;

    bool inWater = false;

    public float BobDistance => bobDistance;

    float initialDrag;

    void Reset()
    {
        parameters = FindUtil.Asset<GlobalParametersSO>();
    }

    void Awake()
    {
        RigidBody = GetComponent<Rigidbody>();
        joint = RigidBody.GetComponent<ConfigurableJoint>();
        collider = GetComponent<Collider>();

        // dont move with parent
        transform.SetParent(null, true);

        initialDrag = RigidBody.drag;
    }

    void OnEnable()
    {
        Visible = true;
        RigidBody.drag = initialDrag;

        joint.linearLimit = new SoftJointLimit()
        {
            limit = bobDistance
        };

        OnHook += OnHookInternal;
    }

    void OnDisable()
    {
        // the RB is still enabled, so fish can still see the RB
        Visible = false;

        if (hookedFish)
        {
            // necessry to remove subscribed callbacks
            Unhook();
        }

        DetachFromRB();
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
            fishingRod.SetHookInRange(true);
        }
        else if(limit > parameters.HookDistancePickupRange)
        {
            fishingRod.SetHookInRange(false);
        }
    }

    public void AttachToRB(Rigidbody rb)
    {
        joint.connectedBody = rb;
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;
    }

    public void DetachFromRB()
    {
        joint.connectedBody = null;
        joint.xMotion = ConfigurableJointMotion.Free;
        joint.yMotion = ConfigurableJointMotion.Free;
        joint.zMotion = ConfigurableJointMotion.Free;
    }

    void OnHookInternal(Fish fish)
    {
        this.fish = fish;
        hookedFish = fish.GetComponent<HookedFish>();
        hookedFish.enabled = true;

        fish.AttachTo(RigidBody);

        collider.enabled = false;
        // since we disable the collider, we don't get any
        // more OnTrigger_ updates, so we should reset the
        // drag here
        RigidBody.drag = initialDrag;

        // other fish should no longer see hook
        Visible = false;

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

    /// <summary>
    /// Set hook to initial state to be cast again
    /// </summary>
    void ResetHook()
    {
        fish = null;
        hookedFish = null;
        collider.enabled = true;
        Visible = false;
    }
}
