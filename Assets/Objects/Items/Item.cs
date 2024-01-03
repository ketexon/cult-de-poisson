using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Item : MonoBehaviour
{
    [SerializeField] ItemSO itemSO;

    public ItemSO ItemSO => itemSO;

    protected GameObject player;
    protected PlayerInput playerInput;
    protected PlayerItem playerItem;
    protected PlayerInteract playerInteract;
    protected Camera mainCamera;

    public virtual void OnUse()
    {}

    public virtual void OnStopUsingItem()
    {
        Destroy(gameObject);
    }

    public virtual void Initialize(
        GameObject player, 
        PlayerInput playerInput, 
        PlayerItem playerItem, 
        PlayerInteract playerInteract,
        Camera mainCamera
    )
    {
        this.player = player;
        this.playerInput = playerInput;
        this.playerItem = playerItem;
        this.playerInteract = playerInteract;
        this.mainCamera = mainCamera;
    }
}
