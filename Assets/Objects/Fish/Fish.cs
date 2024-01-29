using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{
    [SerializeField] public FishSO FishSO;
    protected Rigidbody rb;
    protected BoxCollider boxCollider;

    float _startTime;

    protected float Time => UnityEngine.Time.time - _startTime;

    virtual protected void Awake()
    {
        _startTime = UnityEngine.Time.time;

        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
    }

    /// <summary>
    /// The acceleration the fish causes while caught
    /// Applied every FixedUpdate to the hook of the fishing line
    /// </summary>
    /// <returns></returns>
    public virtual Vector3 ResistanceAcceleration()
    {
        return Vector3.zero;
    }

    public void InitializeBucket()
    {
        Destroy(rb);
        boxCollider.isTrigger = true;
    }
}