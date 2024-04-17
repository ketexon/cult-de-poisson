using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{
    public FishItemBehaviour ItemBehaviour { get; private set; }
    [SerializeField] public FishSO FishSO;
    protected Rigidbody rb;
    protected BoxCollider boxCollider;

    [System.NonSerialized]
    public ConfigurableJoint Joint;

    float _startTime;

    public FishMovement FishMovement { get; protected set; }

    public HookedFish HookedFish { get; protected set; }

    protected float Time => UnityEngine.Time.time - _startTime;

    virtual protected void Awake()
    {
        _startTime = UnityEngine.Time.time;

        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();

        ItemBehaviour = GetComponent<FishItemBehaviour>();
        FishMovement = GetComponent<FishMovement>();
        HookedFish = GetComponent<HookedFish>();

        Joint = GetComponent<ConfigurableJoint>();
    }

    /// <summary>
    /// The acceleration the fish causes while caught
    /// Applied every FixedUpdate to the hook of the fishing line
    /// </summary>
    /// <returns></returns>
    public virtual Vector3 ResistanceAcceleration()
    {
        return Vector3.zero;
    }

    public void InitializeBucket()
    {
        rb.isKinematic = true;
        rb.detectCollisions = false;
        rb.useGravity = false;
        boxCollider.enabled = false;
        if(FishMovement)
        {
            FishMovement.enabled = false;
        }
        if (HookedFish)
        {
            HookedFish.enabled = false;
        }
    }

    public void InitializeWater(FishZone fishZone)
    {
        rb.useGravity = false;
        if (FishMovement)
        {
            FishMovement.enabled = true;
            FishMovement.FishZone = fishZone;
        }
        if (HookedFish)
        {
            HookedFish.enabled = false;
        }
    }

    public void InitializePhysical()
    {
        rb.isKinematic = false;
        rb.detectCollisions = true;
        rb.useGravity = true;
        boxCollider.enabled = true;
        if (FishMovement)
        {
            FishMovement.enabled = false;
        }
        if (HookedFish)
        {
            HookedFish.enabled = false;
        }
    }

    public void AttachTo(Rigidbody rb)
    {
        Joint.connectedBody = rb;

        Joint.xMotion = ConfigurableJointMotion.Locked;
        Joint.yMotion = ConfigurableJointMotion.Locked;
        Joint.zMotion = ConfigurableJointMotion.Locked;
    }

    public void Detach()
    {
        Joint.connectedBody = null;

        Joint.xMotion = ConfigurableJointMotion.Free;
        Joint.yMotion = ConfigurableJointMotion.Free;
        Joint.zMotion = ConfigurableJointMotion.Free;
    }

    public Bounds Bounds => boxCollider.bounds;
}
