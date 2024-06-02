using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PufferfishItemBehaviour : FishItemBehaviour
{
    [SerializeField] float force = 5;
    [SerializeField] float drag = 5;

    public override bool TargetInteractVisible => true;
    public override string TargetInteractMessage => turnedOn ? "to turn on pufferfish :(" : "to turn off pufferfish uwo";

    bool turnedOn = false;

    float initialDrag;

    Rigidbody rb;

    void Awake()
    {
        rb = Player.Instance.GetComponent<Rigidbody>();
        initialDrag = rb.drag;
    }

    void OnDisable()
    {
        Player.Instance.Movement.SetPhysicsEnabled(false);
        rb.drag = initialDrag;
    }

    public override void OnInteract()
    {
        turnedOn = !turnedOn;

        if (turnedOn)
        {
            Player.Instance.Movement.SetPhysicsEnabled(true);
            initialDrag = rb.drag;
            rb.drag = drag;
        }
        else
        {
            Player.Instance.Movement.SetPhysicsEnabled(false);
            rb.drag = initialDrag;
        }

        InteractivityChangeEvent?.Invoke(this);
    }

    void FixedUpdate()
    {
        if (turnedOn)
        {
            rb.AddForce(Vector3.up * force - Physics.gravity);
        }
    }
}
