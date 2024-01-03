using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{
    [SerializeField] public FishSO FishSO;

    float _startTime;

    protected float Time => UnityEngine.Time.time - _startTime;

    virtual protected void Awake()
    {
        _startTime = UnityEngine.Time.time;
    }

    public virtual Vector3 ResistanceAcceleration()
    {
        return Vector3.zero;
    }
}
