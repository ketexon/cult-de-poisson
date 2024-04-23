using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    LineRenderer lineRenderer;
    Vector3 [] positionArr = new Vector3 [2];

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        positionArr[0] = transform.position;
        if(Physics.Raycast(transform.position, transform.forward, out var hitInfo)) {
            positionArr[1] = hitInfo.point; 
        }
        else {
            positionArr[1] = transform.position + transform.forward*7000; 
        }

        lineRenderer.SetPositions(positionArr);
    }
}
