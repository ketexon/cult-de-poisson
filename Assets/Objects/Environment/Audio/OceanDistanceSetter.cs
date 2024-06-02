using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanDistanceSetter : MonoBehaviour
{
    [SerializeField, ParamRef]
    string parameter;

    void Update()
    {
        float minDist = Mathf.Infinity;
        foreach (Transform child in transform)
        {
            minDist = Mathf.Min(minDist, Vector3.Distance(Player.Instance.Movement.Position, child.position));
        }
        Debug.Log(minDist);
        RuntimeManager.StudioSystem.setParameterByName(parameter, minDist);
    }
}
