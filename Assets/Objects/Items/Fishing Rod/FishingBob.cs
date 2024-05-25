using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingBob : MonoBehaviour
{
    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] float waterDrag = 5;
    [SerializeField] float waterBuoyancy = 20;
    Rigidbody rb;
    ConfigurableJoint joint;

    float initialDrag;

    public System.Action CollisionEvent;

    bool inWater = false;

    public void AttachToTip(GameObject tip, float initialLimit)
    {
        joint.connectedBody = tip.GetComponent<Rigidbody>();
        joint.linearLimit = new SoftJointLimit()
        {
            limit = initialLimit,
        };
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;
    }

    public void Reel(float limit)
    {
        joint.linearLimit = new SoftJointLimit()
        {
            limit = limit,
        };
    }

    void Reset()
    {
        parameters = FindUtil.Asset<GlobalParametersSO>();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();

        initialDrag = rb.drag;
    }

    void FixedUpdate()
    {
        if (inWater)
        {
            // acceleration bc we don't want it to depend on mass
            rb.AddForce(Vector3.up * waterBuoyancy, ForceMode.Acceleration);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (parameters.WaterLayerMask.Contains(other.gameObject.layer))
        {
            inWater = true;
            rb.drag = waterDrag;
            CollisionEvent?.Invoke();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (parameters.WaterLayerMask.Contains(other.gameObject.layer))
        {
            inWater = false;
            rb.drag = initialDrag;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        CollisionEvent?.Invoke();
    }
}
