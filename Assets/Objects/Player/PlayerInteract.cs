using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Script to allow player to interact with multiple items.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public class PlayerInteract : MonoBehaviour
{
    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] InputActionReference interactAction;

    /// <summary>
    /// Interactable being hovered over.
    /// This is either an Interactable (eg. an item on the ground)
    /// or added programmatically through code
    /// </summary>
    class InteractTarget
    {
        /// <summary>
        /// Called when this target is "next" to be interacted with
        /// and the interact key is pressed.
        /// </summary>
        public System.Action Callback;

        /// <summary>
        /// Message shown in the UI for the interact target.
        /// For example, when you pick up a fish, it will show "F: Pick up fish" in the bottom left.
        /// "Pick up fish" would be the message.
        /// </summary>
        public string Message;

        /// <summary>
        /// Whether the target is disabled.
        /// This is useful for showing that something can be done,
        /// but not right now.
        /// </summary>
        public bool Disabled;

        /// <summary>
        /// This is used internally to delete the UI text from the screen.
        /// See <c>InputUI</c>
        /// </summary>
        public System.Action InputUIDestructor;
    }

    List<InteractTarget> interactTargets = new();

    Interactable _interactable;

    /// <summary>
    /// The interactable the player is hovering over
    /// </summary>
    Interactable Interactable {
        get => _interactable;
        // Sets the interactable and unsubscribes from the previous
        // interactable's events
        set
        {
            if(value != _interactable)
            {
                if (_interactable)
                {
                    _interactable.CanInteractChangeEvent -= OnInteractableCanInteractChange;
                }
                _interactable = value;
                UpdateUI();
                if (_interactable)
                {
                    _interactable.CanInteractChangeEvent += OnInteractableCanInteractChange;
                }
            }
        }
    }

    PlayerMovement playerMovement;

    System.Action interactableUIDestructor = null;

    void Reset()
    {
        parameters = FindUtil.Asset<GlobalParametersSO>();
    }

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    void OnDestroy()
    {
        if (_interactable)
        {
            _interactable.CanInteractChangeEvent -= OnInteractableCanInteractChange;
        }
    }

    /// <summary>
    /// Input callback
    /// </summary>
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if(!ctx.performed)
        {
            return;
        }
        if (Interactable && Interactable.CanInteract)
        {
            Interactable.OnInteract();
        }
        else
        {
            foreach (var interactTarget in interactTargets)
            {
                if (!interactTarget.Disabled)
                {
                    interactTarget.Callback();
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Add an interact target to the queue of interact targets.
    /// This causes the target's message to be shown on screen and
    /// for its callback to be called once interact is called and
    /// it is first in queue.
    /// </summary>
    /// <param name="callback">Callback that is called when the player interacts with the interactable.</param>
    /// <param name="message">Message to show on screen describing the interactable.</param>
    /// <param name="disabled">Show the message in a disabled color and don't call the callback.</param>
    /// <returns>A destructor to call to remove the interactable text from screen.</returns>
    public System.Action AddInteract(System.Action callback, string message, bool disabled = false)
    {
        InteractTarget target = new()
        {
            Callback = callback,
            Message = message,
            Disabled = disabled,
        };
        interactTargets.Add(target);
        UpdateUI();
        return () =>
        {
            interactTargets.Remove(target);
            target.InputUIDestructor?.Invoke();
        };
    }

    void Update()
    {
        // Check if player is pointing at an interactable
        if(Physics.Raycast(
            transform.position, playerMovement.Camera.transform.forward, 
            out RaycastHit hit, 
            parameters.InteractDistance, 
            parameters.InteractLayerMask,
            QueryTriggerInteraction.Collide // hit triggers
        ))
        {
            Interactable = hit.collider.GetComponent<Interactable>();
        }
        else
        {
            Interactable = null;
        }
    }

    /// <summary>
    /// Called when the interactable we are hovering
    /// changes its "CanUse" state.
    /// This means we need to redraw the UI to show that
    /// the interactable is no longer disabled.
    /// </summary>
    void OnInteractableCanInteractChange(bool _)
    {
        UpdateUI();
    }

    /// <summary>
    /// Basically just redraws the entire UI if we add any new
    /// interactables or one interactable becomes disabled/enabled
    /// </summary>
    void UpdateUI()
    {
        interactableUIDestructor?.Invoke();
        interactableUIDestructor = null;
        bool hasInteractTarget = false;
        if (Interactable)
        {
            interactableUIDestructor = InputUI.Instance.AddInputUI(
                interactAction,
                Interactable.InteractMessage,
                !Interactable.CanInteract
            );
            hasInteractTarget = Interactable.CanInteract;
            InputUI.Instance.SetCrosshairEnabled(Interactable.CanInteract);
        }
        else
        {
            InputUI.Instance.SetCrosshairEnabled(false);
        }
        foreach (var interactTarget in interactTargets)
        {
            interactTarget.InputUIDestructor?.Invoke();
            interactTarget.InputUIDestructor = InputUI.Instance.AddInputUI(
                interactAction,
                interactTarget.Message,
                interactTarget.Disabled || hasInteractTarget
            );
            hasInteractTarget = true;
        }
    }
}
