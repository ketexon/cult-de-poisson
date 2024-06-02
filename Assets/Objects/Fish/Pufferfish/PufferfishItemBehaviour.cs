using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PufferfishItemBehaviour : FishItemBehaviour
{
    [SerializeField] float force = 5;

    public override bool TargetInteractVisible => true;
    public override string TargetInteractMessage => turnedOn ? "to turn on pufferfish :(" : "to turn off pufferfish uwo";

    bool turnedOn = false;

    Rigidbody rb;

    void Awake()
    {
        rb = Player.Instance.GetComponent<Rigidbody>();
    }

    void OnDisable()
    {
        Player.Instance.Movement.SetPhysicsEnabled(false);
    }

    public override void OnInteract()
    {
        turnedOn = !turnedOn;

        if (turnedOn)
        {
            Player.Instance.Movement.SetPhysicsEnabled(true);
        }
        else
        {
            Player.Instance.Movement.SetPhysicsEnabled(false);
        }

        InteractivityChangeEvent?.Invoke(this);
    }

    void FixedUpdate()
    {
        if (turnedOn)
        {
            rb.AddForce(Vector3.up * force - Physics.gravity, ForceMode.Acceleration);
        }
    }
}
