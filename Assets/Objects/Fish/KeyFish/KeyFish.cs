using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyFish : Fish
{
    [SerializeField] float accelerationCycleFrequency = 3;
    [SerializeField] float accelerationSubCycleFrequency = 3;

    [Min(0)]
    [SerializeField] float horizontalAcceleration = 30;

    [Min(0)]
    [SerializeField] float forwardAcceleration = 30;

    public override Vector3 ResistanceAcceleration()
    {
        // 1, 0, -1, 0, 1, 0, -1, ...
        var cos = Mathf.Cos(Time / accelerationCycleFrequency * 2 * Mathf.PI) * Mathf.Cos(Time / accelerationCycleFrequency / accelerationSubCycleFrequency * 2 * Mathf.PI);
        return new Vector3(
            cos * horizontalAcceleration,
            0,
            forwardAcceleration
        );
    }
}
