using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour
{
    [SerializeField] Transform targetPoint;
    [SerializeField] float strength;

    bool inWind = false;
    Rigidbody RB => Player.Instance.Movement.Rigidbody;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("HI");
        inWind = true;
    }

    void OnTriggerExit(Collider other)
    {
        inWind = false;
    }

    void FixedUpdate()
    {
        if (inWind)
        {
            RB.AddForce((targetPoint.position - RB.position) * strength);
        }
    }
}
