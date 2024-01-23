using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BucketFish : MonoBehaviour
{
    [SerializeField] float translateSpeed = 10f;
    [SerializeField] float rotateSpeed = 10f;

    public Vector3 StartLocalPos { get; private set; }
    public Quaternion StartLocalRotation { get; private set; }
    [System.NonSerialized] public Vector3 TargetLocalPos;
    [System.NonSerialized] public Quaternion TargetLocalRotation;

    void Awake()
    {
        StartLocalPos = TargetLocalPos = transform.localPosition;
        StartLocalRotation = TargetLocalRotation = transform.localRotation;
    }

    void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, TargetLocalPos, Time.deltaTime * rotateSpeed);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, TargetLocalRotation, Time.deltaTime * rotateSpeed);
    }
}
