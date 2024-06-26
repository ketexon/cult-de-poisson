using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{
    public FishItemBehaviour ItemBehaviour { get; private set; }
    public FishInteractable FishInteractable { get; private set; }
    public FishMovement FishMovement { get; protected set; }
    public HookedFish HookedFish { get; protected set; }


    [SerializeField] public FishSO FishSO;
    protected Rigidbody rb;
    protected BoxCollider boxCollider;

    [System.NonSerialized]
    public ConfigurableJoint Joint;

    float _startTime;

    protected float Time => UnityEngine.Time.time - _startTime;

    virtual protected void Awake()
    {
        _startTime = UnityEngine.Time.time;

        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();

        ItemBehaviour = GetComponent<FishItemBehaviour>();
        FishMovement = GetComponent<FishMovement>();
        HookedFish = GetComponent<HookedFish>();
        FishInteractable = GetComponent<FishInteractable>();

        Joint = GetComponent<ConfigurableJoint>();

        InitializePhysical();
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
        if(FishMovement)
        {
            FishMovement.enabled = false;
        }
        if (HookedFish)
        {
            HookedFish.enabled = false;
        }
        if (FishInteractable)
        {
            FishInteractable.enabled = false;
        }

        rb.isKinematic = true;
        rb.detectCollisions = false;
        rb.useGravity = false;
        boxCollider.enabled = false;
    }

    public void InitializeWater(FishZone fishZone)
    {
        if (FishMovement)
        {
            FishMovement.enabled = true;
            FishMovement.FishZone = fishZone;
        }
        if (HookedFish)
        {
            HookedFish.enabled = false;
        }
        if (FishInteractable)
        {
            FishInteractable.enabled = false;
        }

        rb.isKinematic = false;
        rb.detectCollisions = true;
        rb.useGravity = false;
        boxCollider.enabled = true;
    }

    public void InitializePhysical()
    {
        if (FishMovement)
        {
            FishMovement.enabled = false;
        }
        if (HookedFish)
        {
            HookedFish.enabled = false;
        }
        if (FishInteractable)
        {
            FishInteractable.enabled = true;
        }

        rb.isKinematic = false;
        rb.detectCollisions = true;
        rb.useGravity = true;
        boxCollider.enabled = true;
    }

    public void InitializeStatic()
    {
        if (FishMovement)
        {
            FishMovement.enabled = false;
        }
        if (HookedFish)
        {
            HookedFish.enabled = false;
        }
        if (FishInteractable)
        {
            FishInteractable.enabled = false;
        }

        rb.isKinematic = true;
        rb.detectCollisions = false;
        rb.useGravity = false;
        boxCollider.enabled = false;
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