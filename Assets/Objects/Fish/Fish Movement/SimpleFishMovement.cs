using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Fish))]
public class SimpleFishMovement : FishMovement
{
    [SerializeField] GameObject fishVisionPrefab;
    [SerializeField] float visionSize = 1.0f;
    [SerializeField] float idleSpeed = 2.0f;
    [SerializeField] float maxRotation = 2.0f;
    [SerializeField] float collisionDistance = 1.0f;

    FishVision fishVision;

    Vector3 actualDirection;
    Vector3 targetDirection;
    float speed;

    FishingHook hook;

    void Awake()
    {
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

    void OnDestroy()
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

        Debug.Log(hook);
    }

    void OnHookInvisible()
    {
        hook = null;
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
        } while (FishWillCollide() && ++nTries < 5);
    }

    /// <summary>
    /// returns true if the fish will not collide with the edge
    /// of the fishzone within collisiondistance
    /// </summary>
    /// <returns></returns>
    bool FishWillCollide()
    {
        return !FishZone.Contains(transform.position + targetDirection * speed);
    }


    void Update()
    {
        // if the hook exists and is inside the fish zone, go towards it
        if (hook && FishZone.Contains(hook.transform.position))
        {
            targetDirection = transform.position - hook.transform.position;
        }
        else if(FishWillCollide())
        {
            GenerateNewDirection();
        }

        actualDirection = Vector3.Lerp(actualDirection, targetDirection, Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(actualDirection, Vector3.up);
        transform.position += actualDirection * Time.deltaTime;
    }
}
