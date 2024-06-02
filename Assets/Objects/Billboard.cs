using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    Quaternion initialRot;

    void Awake()
    {
        initialRot = transform.rotation;
    }

    void Update()
    {
        transform.rotation = Quaternion.FromToRotation(Vector3.forward, (Player.Instance.transform.position - transform.position).ProjectXZ());
    }
}
