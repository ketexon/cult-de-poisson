using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(ConfigurableJoint))]
public class FishingHook : MonoBehaviour
{
    [SerializeField] LayerMask waterLayerMask;
    [SerializeField] LayerMask fishingZoneLayerMask;
    [SerializeField] float waterDrag = 5.0f;
    Rigidbody rb;
    ConfigurableJoint joint;

    bool inWater = false;

    public void OnBobHitWater(FishingBob bob, float distance, float bouncieness)
    {
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = Vector3.zero;
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;
        joint.linearLimit = new SoftJointLimit() { limit = distance, bounciness = bouncieness };
        joint.connectedBody = bob.GetComponent<Rigidbody>();
    }

    void Reset()
    {
        waterLayerMask = FindUtil.Layer("water");
        fishingZoneLayerMask = FindUtil.Layer("fishzone");
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();
    }

    void OnCollisionEnter(Collision collision)
    {
        rb.isKinematic = true;
    }

    void OnTriggerEnter(Collider other)
    {
        int otherLayerField = 1 << other.gameObject.layer;
        if ((otherLayerField & waterLayerMask.value) > 0)
        {
            rb.velocity = Vector3.zero;
            rb.drag = waterDrag;
        }
    }
}
