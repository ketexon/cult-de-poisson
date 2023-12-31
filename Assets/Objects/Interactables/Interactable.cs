using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [SerializeField] bool _interactable = true;
    public bool CanInteract
    {
        get => _interactable;
        protected set
        {
            if(_interactable != value)
            {
                _interactable = value;
                CanInteractChangeEvent?.Invoke(_interactable);
            }
        }
    }

    public System.Action<bool> CanInteractChangeEvent;

    public abstract string InteractMessage { get; }
    public virtual void OnInteract() { }
}
