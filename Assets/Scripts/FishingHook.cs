using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(ConfigurableJoint))]
public class FishingHook : MonoBehaviour
{
    [System.NonSerialized]
    public PlayerFish PlayerFish;

    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] float waterDrag = 5.0f;
    [SerializeField] float bobDistance = 5.0f;
    [SerializeField] Fish fish = null;
    
    Rigidbody rb;
    ConfigurableJoint joint;

    bool inWater = false;

    public float BobDistance => bobDistance;

    float initialDrag;

    public System.Action<Vector3> WaterHitEvent;
    public System.Action<Fish> FishCatchEvent;
    public Vector3? WaterHitPos { get; private set; }

    public void OnCatchFish(FishSO fish)
    {
        var fishGO = Instantiate(fish.Prefab, transform);
        this.fish = fishGO.GetComponent<Fish>();
        FishCatchEvent?.Invoke(this.fish);
    }

    void Reset()
    {
        parameters = FindUtil.Asset<GlobalParametersSO>();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        joint = rb.GetComponent<ConfigurableJoint>();

        initialDrag = rb.drag;

        joint.linearLimit = new SoftJointLimit() {
            limit = bobDistance
        };
    }

    void FixedUpdate()
    {
        if (fish && inWater)
        {
            rb.AddForce(fish.ResistanceAcceleration(), ForceMode.Acceleration);            
        }
    }

    public void Reel(float limit)
    {
        joint.linearLimit = new SoftJointLimit()
        {
            limit = limit
        };
    }

    public void AttachToRB(Rigidbody rb)
    {
        joint.connectedBody = rb;
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!fish)
        {
            //rb.isKinematic = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        int otherLayerField = 1 << other.gameObject.layer;
        if ((otherLayerField & parameters.WaterLayerMask.value) > 0)
        {
            WaterHitPos = transform.position;
            WaterHitEvent?.Invoke(WaterHitPos.Value);
            //rb.velocity = Vector3.zero;
            rb.drag = waterDrag;
            inWater = true;
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
}
