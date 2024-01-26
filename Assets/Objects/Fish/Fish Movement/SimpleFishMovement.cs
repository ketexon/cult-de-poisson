using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFishMovement : FishMovement
{
    [SerializeField] float minWaitTime = 1.0f;
    [SerializeField] float maxWaitTime = 3.0f;
    [SerializeField] float speed = 1.0f;
    [SerializeField] float rotateSpeed = 1.0f;

    enum State
    {
        Waiting,
        Swimming
    }

    State state = State.Waiting;
    Vector3 targetPoint;
    Quaternion targetRotation;

    float endWaitTime;

    Vector3 startSwimPos;
    float startSwimTime;
    float endSwimTime;

    void Start()
    {
        endWaitTime = Time.time;
    }

    void Update()
    {
        if (state == State.Waiting && Time.time >= endWaitTime)
        {
            StartSwimming();
        }
        if(state == State.Swimming)
        {
            if(Time.time >= endSwimTime)
            {
                StartWaiting();
            }
            else
            {
                var t = (Time.time - startSwimTime) / (endSwimTime - startSwimTime);
                transform.position = Vector3.Lerp(startSwimPos, targetPoint, t);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
            }
        }
    }

    void StartSwimming()
    {
        state = State.Swimming;

        startSwimPos = transform.position;
        startSwimTime = Time.time;

        targetPoint = FishZone.GetRandomPoint();
        targetRotation = Quaternion.LookRotation(targetPoint - transform.position, Vector3.up);

        endSwimTime = Time.time + (targetPoint - startSwimPos).magnitude / speed;
    }

    void StartWaiting()
    {
        transform.position = targetPoint;
        transform.rotation = targetRotation;

        state = State.Waiting;

        endWaitTime = Time.time + Random.Range(minWaitTime, maxWaitTime);
    }
}
