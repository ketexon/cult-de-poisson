using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingBob : MonoBehaviour
{
    [SerializeField] LayerMask waterLayerMask;
    Rigidbody rb;

    FishingLineOld line;

    public void SetLine(FishingLineOld line)
    {
        this.line = line;
    }

    void Reset()
    {
        waterLayerMask = FindUtil.Layer("water");
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other)
    {
        var otherLayerMask = 1 << other.gameObject.layer;
        if ((otherLayerMask & waterLayerMask.value) > 0)
        {
            rb.isKinematic = true;
            line.OnBobHitWater();
        }
    }
}
