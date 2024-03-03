using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Base class for all item scripts.
/// </summary>
public class Item : MonoBehaviour
{
    /// <summary>
    /// The ItemSO corresponding to the Item.
    /// Used to compare the Item for equality and
    /// for item parameters like name.
    /// </summary>
    [SerializeField] ItemSO itemSO;

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
