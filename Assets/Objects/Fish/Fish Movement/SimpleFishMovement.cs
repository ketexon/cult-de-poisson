using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Fish), typeof(Rigidbody), typeof(Collider))]
public class SimpleFishMovement : FishMovement
{
    [SerializeField] GameObject fishVisionPrefab;
    [SerializeField] float visionSize = 1.0f;
    [SerializeField] float idleSpeed = 2.0f;
    [SerializeField] float chaseHookSpeed = 4.0f;
    [SerializeField] float maxRotation = 2.0f;
    [SerializeField] float collisionDistance = 1.0f;

    FishVision fishVision;

    Vector3 actualDirection;
    Vector3 targetDirection;
    float speed;

    FishingHook hook;

    Rigidbody rb;
    new BoxCollider collider;

    override protected void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody>();
        collider = GetComponent<BoxCollider>();
    }

    void Start()
    {
        GenerateNewDirection();
        speed = idleSpeed;

        var fishVisionGO = Instantiate(fishVisionPrefab, transform, false);
        fishVision = fishVisionGO.GetComponent<FishVision>();
        fishVision.SetSize(visionSize);

        fishVision.HookVisibleEvent += OnHookVisible;
        fishVision.HookInvisibleEvent += OnHookInvisible;
    }

    override protected void OnDestroy()
    {
        if (fishVision)
        {
            fishVision.HookVisibleEvent -= OnHookVisible;
            fishVision.HookInvisibleEvent -= OnHookInvisible;
        }
    }

    void OnHookVisible(FishingHook hook)
    {
        this.hook = hook;
        speed = chaseHookSpeed;
    }

    void OnHookInvisible()
    {
        hook = null;
        speed = idleSpeed;
    }

    public override void Unhook()
    {
        base.Unhook();

        hook = null;
        speed = idleSpeed;
    }

    /// <summary>
    /// Tries to generate a direction that does not collide.
    /// Stops after 5 tries to prevent infinite loop.
    /// </summary>
    void GenerateNewDirection()
    {
        int nTries = 0;
        do
        {
            targetDirection = Extensions.Random(-Vector3.one, Vector3.one).normalized;
        } while (FishWillCollide() && ++nTries < 3);
    }

    /// <summary>
    /// returns true if the fish will not collide with the edge
    /// of the fishzone within collisiondistance
    /// </summary>
    /// <returns></returns>
    bool FishWillCollide()
    {
        bool inBounds = FishZone.Contains(transform.position + targetDirection * collisionDistance);
        bool willCollide = Physics.BoxCast(
            collider.center + transform.position,
            collider.size,
            targetDirection,
            transform.rotation,
            collisionDistance,
            parameters.GroundLayerMask
        );
        return !inBounds || willCollide;
    }

    // enable fishVision when we are enabled
    void OnEnable()
    {
        if (fishVision)
        {
            fishVision.enabled = true;
        }
    }

    // disable fishVision when we are disabled
    void OnDisable()
    {
        if (fishVision)
        {
            fishVision.enabled = false;
        }
    }

    void FixedUpdate()
    {
        if (!enabled) return;
        var playerPos = Player.Instance.Movement.transform.position;
        if (Vector3.Distance(playerPos, transform.position) > 80)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        // if the hook exists and is inside the fish zone, go towards it
        if (hook && FishZone.Contains(hook.transform.position))
        {
            targetDirection = (hook.transform.position - transform.position).normalized;
        }
        // otherwise, go forward unless going forward will collide
        // if it will, generate a new direction so we don't collide
        else if(FishWillCollide())
        {
            GenerateNewDirection();
        }

        // lerp rotation and direction and apply to transform and RB
        actualDirection = Vector3.Lerp(actualDirection, targetDirection, Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(actualDirection, Vector3.up);
        rb.velocity = actualDirection * speed;
    }

    // remove gravity if fish in water
    void OnTriggerEnter(Collider other)
    {
        if (parameters.WaterLayerMask.Contains(other.gameObject.layer))
        {
            rb.useGravity = false;
        }
    }

    // add gravity if fish leaves water
    void OnTriggerExit(Collider other)
    {

        if (parameters.WaterLayerMask.Contains(other.gameObject.layer))
        {
            rb.useGravity = true;
        }
    }
}
