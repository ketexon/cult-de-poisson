using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Base class for all item scripts.
/// </summary>
public class Item : MonoBehaviour, IInteractItem
{
    /// <summary>
    /// The ItemSO corresponding to the Item.
    /// Used to compare the Item for equality and
    /// for item parameters like name.
    /// </summary>
    [SerializeField] ItemSO itemSO;

    public virtual bool CanSwitchItems => true;

    public ItemSO ItemSO => itemSO;

    protected GameObject player;
    protected PlayerInput playerInput;
    protected PlayerItem playerItem;
    protected PlayerInteract playerInteract;
    protected Camera mainCamera;
    protected CinemachineBrain cinemachineBrain;

    public class InitializeParams
    {
        public GameObject Player;
        public PlayerInput PlayerInput;
        public PlayerItem PlayerItem;
        public PlayerInteract PlayerInteract;
        public Camera MainCamera;
        public CinemachineBrain CinemachineBrain;
    };

    public virtual bool InteractVisible => false;

    public virtual bool InteractEnabled => false;

    public virtual string InteractMessage => null;

    /// <summary>
    /// If true, the item and the current interactable
    /// are treated as one interaction. This primarily
    /// does not show the message of the interactable, if it exists.
    /// Eg. if this is the KeyFish and the Interactable is the door
    ///     by default, 
    ///         the KeyFish would have no message
    ///         the door would say "Door is locked"
    ///     if looking at the door with the keyfish
    ///         the KeyFish would have the message "Unlock door"
    ///         the door *should have no message*
    ///     If InteractsWithInteractable is false, the door's message
    ///     will automatically be deleted to prevent coupling
    /// </summary>
    public virtual bool InteractsWithInteractable => false;

    public virtual void OnInteract(Interactable target) { }

    /// <summary>
    /// Called when the UseItem key is pressed.
    /// </summary>
    public virtual void OnUse()
    {}


    /// <summary>
    /// Called when the player switches items
    /// </summary>
    public virtual void OnStopUsingItem()
    {
        Destroy(gameObject);
    }

    public virtual void Initialize(InitializeParams initParams)
    {
        player = initParams.Player;
        playerInput = initParams.PlayerInput;
        playerItem = initParams.PlayerItem;
        playerInteract = initParams.PlayerInteract;
        mainCamera = initParams.MainCamera;
        cinemachineBrain = initParams.CinemachineBrain;
    }
}
