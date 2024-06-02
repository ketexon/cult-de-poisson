using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectedEel : MonoBehaviour
{
    [SerializeField] GameObject eelPrefab;
    [SerializeField] float strengthRatio = 0.8f;
    [SerializeField] Transform point1;
    [SerializeField] Transform point2;

    void Awake()
    {
        Vector3 p1 = point1.position;
        Vector3 p2 = point2.position;
        Vector3 MidPoint = (p1 + p2) / 2;
        float Stretch = Vector3.Distance(p1, p2) * strengthRatio;
        var go = Instantiate(eelPrefab, MidPoint, Quaternion.identity);
        var fish = go.GetComponent<Fish>();
        Quaternion Rotation = Quaternion.FromToRotation(fish.transform.forward, p2 - p1);

        fish.InitializeStatic();
        fish.transform.rotation = fish.transform.rotation * Rotation;
        fish.transform.localScale = new Vector3(1, 1, Stretch);
    }
}
