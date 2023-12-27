using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{
    [SerializeField] public FishSO FishSO;

    public virtual Vector3 ResistanceAcceleration()
    {
        return Vector3.zero;
    }
}
