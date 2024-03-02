using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SerializedMeshInteractable : Interactable
{
    [SerializeField] string message;

    public override string InteractMessage => message;
}
