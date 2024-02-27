using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [SerializeField] protected new Collider collider;
    [SerializeField] bool _interactable = true;

    /// <summary>
    /// Used by <c>PlayerIntact</c> to show interact text but not
    /// allow interaction. Useful for disabled interactables.
    /// </summary>
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

    protected virtual void Awake()
    {
        collider = GetComponent<Collider>();
    }

    protected virtual void OnEnable()
    {
        collider.enabled = true;
    }

    protected virtual void OnDisable()
    {
        collider.enabled = false;
    }

    /// <summary>
    /// Called when CanInteract is changed.
    /// </summary>
    public System.Action<bool> CanInteractChangeEvent;

    /// <summary>
    /// Message to show in UI when hovering this item
    /// </summary>
    public abstract string InteractMessage { get; }

    public virtual void OnInteract() { }
}
