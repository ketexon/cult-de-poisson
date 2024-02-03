using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFishMovement : FishMovement
{
    [SerializeField] float idleSpeed = 2.0f;
    [SerializeField] float maxRotation = 2.0f;
    [SerializeField] float radiusOfCurvatureMult = 1.0f;

    Vector3 velocity;

    float lastTailRotation;

    float tailRotation = 0;
    float tailRotationalVelocity = 0;
    float tailRotationalAcceleration = 0;

    float actualTailRotationalVelocity => 
        tailRotationalVelocity * (1 - Mathf.Abs(tailRotation) / maxRotation);



    void Start()
    {
    }

    void Update()
    {
        tailRotation += actualTailRotationalVelocity * Time.deltaTime;
        tailRotation = Mathf.Clamp(tailRotation, -maxRotation, maxRotation);


    }
}
