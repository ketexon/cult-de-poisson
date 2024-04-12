using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [SerializeField] bool _interactEnabled = true;
    bool _interactVisible = true;

    protected new Collider collider;
    
    /// <summary>
    /// Used by <c>PlayerIntact</c> to show interact text but not
    /// allow interaction. Useful for disabled interactables.
    /// </summary>
    public virtual bool InteractEnabled
    {
        get => _interactEnabled;
        protected set
        {
            if(_interactEnabled != value)
            {
                _interactEnabled = value;
                InteractDisabledChangedEvent?.Invoke(_interactEnabled);
            }
        }
    }

    /// <summary>
    /// Called when CanInteract is changed.
    /// </summary>
    public System.Action<bool> InteractDisabledChangedEvent;

    /// <summary>
    /// Used by <c>PlayerIntact</c> to show interact text but not
    /// allow interaction. Useful for disabled interactables.
    /// </summary>
    public virtual bool InteractVisible
    {
        get => _interactVisible;
        protected set
        {
            if (_interactVisible != value)
            {
                _interactVisible = value;
                InteractVisibleChangedEvent?.Invoke(_interactVisible);
            }
        }
    }

    /// <summary>
    /// Called when CanInteract is changed.
    /// </summary>
    public System.Action<bool> InteractVisibleChangedEvent;

    /// <summary>
    /// Message to show in UI when hovering this item
    /// </summary>
    public abstract string InteractMessage { get; }

    public virtual void OnInteract() { }
}
