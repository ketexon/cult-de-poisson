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
        Debug.Log("Entered wind");
        inWind = true;
        AudioManager.Instance.PlayWindSound();
    }

    void OnTriggerExit(Collider other)
    {
        inWind = false;

        AudioManager.Instance.StopWindSound();
    }

    void FixedUpdate()
    {
        if (inWind)
        {
            RB.AddForce((targetPoint.position - RB.position).normalized * strength);
        }
    }
}
