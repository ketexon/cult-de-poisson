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

    public float BobDistance => bobDistance;

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
    }

    void Update()
    {
        if(WaterHitPos.HasValue)
        {
            if((transform.position - WaterHitPos.Value).magnitude > bobDistance)
            {
                transform.position = WaterHitPos.Value + (transform.position - WaterHitPos.Value).normalized * bobDistance;
                rb.isKinematic = true;
            }

            if (fish)
            {
                var rot = Quaternion.FromToRotation(
                    Vector3.forward,
                    (transform.position - PlayerFish.transform.position).ProjectXZ()
                );
                var disp = rot * fish.ResistanceVelocity() * Time.deltaTime;
                transform.position += disp;
                WaterHitPos += disp / 2;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!fish)
        {
            rb.isKinematic = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        int otherLayerField = 1 << other.gameObject.layer;
        if ((otherLayerField & parameters.WaterLayerMask.value) > 0)
        {
            WaterHitPos = transform.position;
            WaterHitEvent?.Invoke(WaterHitPos.Value);
            rb.velocity = Vector3.zero;
            rb.drag = waterDrag;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (parameters.WaterLayerMask.Contains(other.gameObject.layer))
        {
            rb.drag = 0;
        }
    }
}
