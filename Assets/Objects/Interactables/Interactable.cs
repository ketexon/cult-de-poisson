using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour, IInteractTarget
{
    [SerializeField] bool _interactEnabled = true;
    bool _interactVisible = true;
    [SerializeField] protected new Collider collider;
    
    /// <summary>
    /// Used by <c>PlayerIntact</c> to show interact text but not
    /// allow interaction. Useful for disabled interactables.
    /// </summary>
    public virtual bool TargetInteractEnabled
    {
        get => _interactEnabled;
        protected set
        {
            if(_interactEnabled != value)
            {
                _interactEnabled = value;
                InteractivityChangeEvent?.Invoke(this);
            }
        }
    }

    protected virtual void Reset()
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
    /// Used by <c>PlayerIntact</c> to show interact text but not
    /// allow interaction. Useful for disabled interactables.
    /// </summary>
    public virtual bool TargetInteractVisible
    {
        get => _interactVisible;
        protected set
        {
            if (_interactVisible != value)
            {
                _interactVisible = value;
                InteractivityChangeEvent?.Invoke(this);
            }
        }
    }


    public System.Action<IInteractObject> InteractivityChangeEvent { get; set; }

    /// <summary>
    /// Message to show in UI when hovering this item
    /// </summary>
    public abstract string TargetInteractMessage { get; }

    public virtual void OnInteract() { }
}
