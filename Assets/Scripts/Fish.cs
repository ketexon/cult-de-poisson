using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{
    public virtual Vector3 ResistanceVelocity()
    {
        return Vector3.zero;
    }
}
