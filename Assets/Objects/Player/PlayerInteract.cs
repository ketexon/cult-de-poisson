using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

/// <summary>
/// An object that participates in an interaction. 
/// Either an an <see cref="IInteractAgent"/> or an <see cref="IInteractTarget"/>. 
/// </summary>
public interface IInteractObject
{ 
    /// <summary>
    /// Callback to let <see cref="PlayerInteract"/> know that some property
    /// affecting interactivity has changed.
    /// Eg. after you make an interact target invisible, you should invoke this.
    /// </summary>
    System.Action<IInteractObject> InteractivityChangeEvent { get; set; }
}

/// <summary>
/// An object being interacted with.
/// See <see cref="PlayerInteract"/>
/// </summary>
public interface IInteractTarget : IInteractObject
{
    bool TargetInteractVisible { get; }
    bool TargetInteractEnabled { get; }
    string TargetInteractMessage { get; }

    void OnInteract();
}

/// <summary>
/// An object that interacts with an See <see cref="IInteractTarget"/>.
/// See <see cref="PlayerInteract"/>
/// </summary>
public interface IInteractAgent : IInteractObject
{
    bool AgentInteractVisible(Interactable target);
    bool AgentInteractEnabled(Interactable target);
    string AgentInteractMessage(Interactable target);

    void OnInteract(Interactable target) { }
}

/// <summary>
/// Interface to allow an object to defer interact
/// behavior to another object. For example, it is 
/// used in <see cref="Item.InteractItem"/> for <see cref="FishItem"/> to defer
/// interact behaviour to the <see cref="Fish.ItemBehaviour"/> of the currently held fish.
/// </summary>
public interface IInteractTargetProxy
{
    IInteractObject InteractItem { get; }
}

/// <summary>
/// <para>
///     Script that handles main player interaction throug the F key.
/// </para>
/// <para>
///     There are two types of interacting components (<see cref="IInteractObject"/>): the <see cref="IInteractAgent"/> and the <see cref="IInteractTarget"/>.
///     The target is the thing being interacted with (eg. a door), and the agent is the "tool" (eg. the key).
/// </para>
/// <para>By default, it checks 4 places for interactions:</para>
/// <list type="bullet">
///     <item>The item the player is holding as an agent to the interactable the player is looking at</item>
///     <item>The interactable the player is looking at as a target without an agent</item>
///     <item>The item the player is holding as a target without an agent</item>
///     <item>Any interactions registered by other scripts</item>
/// </list>
/// <para>
///     See <see cref="KeyFishItemBehaviour"/> for an example of an Agent.
///     See <see cref="Interactable"/> for the interact targets.
/// </para>
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public class PlayerInteract : SingletonBehaviour<PlayerInteract>
{
    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] InputActionReference interactAction;

    /// <summary>
    /// Interactable being hovered over.
    /// This is either an Interactable (eg. an item on the ground)
    /// or added programmatically through code
    /// </summary>
    class ExternalInteractTarget : IInteractTarget
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

        public bool TargetInteractVisible => true;

        public bool TargetInteractEnabled => !Disabled;

        public string TargetInteractMessage => Message;

        public void OnInteract() => Callback?.Invoke();

        public System.Action<IInteractObject> InteractivityChangeEvent { get; set; }

    }

    List<ExternalInteractTarget> externalInteractTargets = new();

    Interactable _interactable;

    /// <summary>
    /// The interactable the player is hovering over
    /// </summary>
    public Interactable Interactable {
        get => _interactable;
        // Sets the interactable and unsubscribes from the previous
        // interactable's events
        set
        {
            if(value != _interactable)
            {
                if (_interactable)
                {
                    _interactable.InteractivityChangeEvent -= OnInteractObjectStateChange;
                }
                _interactable = value;
                UpdateInteractivity();
                if (_interactable)
                {
                    _interactable.InteractivityChangeEvent += OnInteractObjectStateChange;
                }
            }
        }
    }

    // The curent interact target and agent
    // as of refresh
    IInteractTarget interactTarget = null;
    IInteractAgent interactAgent = null;

    PlayerMovement playerMovement;
    PlayerItem playerItem;

    Item enabledItem = null;
    IInteractObject InteractItem => enabledItem ? enabledItem.InteractItem : null;

    System.Action interactableUIDestructor = null;
    System.Action itemUIDestructor = null;

    void Reset()
    {
        parameters = FindUtil.Asset<GlobalParametersSO>();
    }

    override protected void Awake()
    {
        base.Awake();

        playerMovement = GetComponent<PlayerMovement>();
        playerItem = GetComponent<PlayerItem>();
    }

    void Start()
    {
        playerItem.ItemChangeEvent += OnItemChange;

        enabledItem = playerItem.EnabledItem;

        InteractItem.InteractivityChangeEvent += OnInteractObjectStateChange;
        UpdateInteractivity();
    }

    void OnDestroy()
    {
        if (playerItem)
        {
            playerItem.ItemChangeEvent -= OnItemChange;
        }

        if (enabledItem)
        {
            InteractItem.InteractivityChangeEvent -= OnInteractObjectStateChange;
        }

        if (_interactable)
        {
            _interactable.InteractivityChangeEvent -= OnInteractObjectStateChange;
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
        if (interactAgent != null)
        {
            interactAgent.OnInteract(Interactable);
        }
        else if (interactTarget != null)
        {
            interactTarget.OnInteract();
        }
        else
        {
            foreach (var interactTarget in externalInteractTargets)
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
        ExternalInteractTarget target = new()
        {
            Callback = callback,
            Message = message,
            Disabled = disabled,
        };
        externalInteractTargets.Add(target);
        UpdateInteractivity();
        return () =>
        {
            externalInteractTargets.Remove(target);
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
            var interactable = hit.collider.GetComponent<Interactable>();
            if (interactable.enabled)
            {
                Interactable = interactable;
            }
            else
            {
                Interactable = null;
            }
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
    void OnInteractObjectStateChange(IInteractObject _)
    {
        UpdateInteractivity();
    }

    /// <summary>
    /// Callback for <see cref="PlayerItem.ItemChangeEvent"/>
    /// </summary>
    void OnItemChange(Item newItem)
    {
        if (InteractItem != null)
        {
            InteractItem.InteractivityChangeEvent -= OnInteractObjectStateChange;
        }
        enabledItem = newItem;
        if (InteractItem != null)
        {
            InteractItem.InteractivityChangeEvent += OnInteractObjectStateChange;
        }
        
        UpdateInteractivity();
    }

    /// <summary>
    /// Basically just redraws the entire UI if we add any new
    /// interactables or one interactable becomes disabled/enabled
    /// </summary>
    void UpdateInteractivity()
    {
        // if InputUI has been destroyed,
        // then don't actually update
        if (!InputUI.Instance)
        {
            return;
        }
        
        // CLEAN UP CURRENT UI
        interactableUIDestructor?.Invoke();
        interactableUIDestructor = null;

        itemUIDestructor?.Invoke();
        itemUIDestructor = null;

        interactAgent = null;
        interactTarget = null;

        /// whether there is already an interact target
        /// all remaining interact targets will be disabled
        /// to make sure only one callback is pressed per [INTERACT]
        /// keypress
        /// eg. if you can interact with a door or use your fish, 
        ///         the door will be enabled first, since it is an interactable
        ///         This will set hasInteractTarget to true
        ///         Now, picking using the fish will be disabled
        bool hasInteractTarget = false;

        /// set to false if the item can be used
        /// *on* the interactable
        bool agentInteractEnabled = false;

        bool showCrosshair = false;

        // Register agent interact for PlayerItem
        if (InteractItem is IInteractAgent agent && agent.AgentInteractVisible(Interactable))
        {
            bool enabled = agent.AgentInteractEnabled(Interactable);

            itemUIDestructor = InputUI.Instance.AddInputUI(
                interactAction,
                agent.AgentInteractMessage(Interactable),
                disabled: !enabled || hasInteractTarget
            );

            // only show the interactable's message
            // if the interact item *does not* interact
            // with that interactable
            if (enabled && !hasInteractTarget)
            {
                hasInteractTarget = true;

                agentInteractEnabled = true;

                interactAgent = agent;
                
                showCrosshair = true;
            }
        }

        // Register object interact for interactable
        if (!agentInteractEnabled && Interactable && Interactable.TargetInteractVisible)
        {
            bool enabled = Interactable.TargetInteractEnabled;

            interactableUIDestructor = InputUI.Instance.AddInputUI(
                interactAction,
                Interactable.TargetInteractMessage,
                disabled: !enabled || hasInteractTarget
            );

            if (enabled && !hasInteractTarget)
            {
                hasInteractTarget = true;
                
                interactTarget = Interactable;

                showCrosshair = true;
            }
        }

        InputUI.Instance.SetCrosshairEnabled(showCrosshair);

        // register object interact for item
        // Register agent interact for PlayerItem
        if (!agentInteractEnabled && InteractItem is IInteractTarget obj && obj.TargetInteractVisible)
        {
            bool enabled = obj.TargetInteractEnabled;

            itemUIDestructor = InputUI.Instance.AddInputUI(
                interactAction,
                obj.TargetInteractMessage,
                disabled: !enabled || hasInteractTarget
            );

            if (enabled && !hasInteractTarget)
            {
                hasInteractTarget = true;

                interactTarget = obj;
            }
        }


        // Register other interact targets supplied
        foreach (var interactTarget in externalInteractTargets)
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
