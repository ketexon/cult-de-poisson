using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carp : Fish
{
    [SerializeField] float velocityCycleFrequency = 3;

    [Min(0)]
    [SerializeField] float horizontalVelocity = 2;

    [Min(0)]
    [SerializeField] float forwardVelocity = 2;
    float startTime;

    float Time => UnityEngine.Time.time - startTime;

    void Awake()
    {
        startTime = UnityEngine.Time.time;
    }

    public override Vector3 ResistanceVelocity()
    {
        // 1, 0, -1, 0, 1, 0, -1, ...
        var cos = Mathf.Cos(Time / velocityCycleFrequency * 2 * Mathf.PI);
        return new Vector3(
            Mathf.Floor(Mathf.Abs(cos * 2)) * Mathf.Sign(cos) / 2 * horizontalVelocity,
            0,
            forwardVelocity
        );
    }
}
