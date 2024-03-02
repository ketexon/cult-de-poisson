using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Class that contains references to monobehaviours
/// relevant to the player
/// </summary>
public class Player : SingletonBehaviour<Player>
{
    public DialogueManager DialogueManager;
    public PlayerInput Input;
    public PlayerMovement Movement;
    public PlayerItem Item;
    public PlayerInteract Interact;
    public Camera Camera;
    public CinemachineBrain CinemachineBrain;

    void Reset()
    {
        Input = GetComponentInChildren<PlayerInput>();
        Item = GetComponentInChildren<PlayerItem>();
        Interact = GetComponentInChildren<PlayerInteract>();
        Camera = GetComponentInChildren<Camera>();
        if (Camera)
        {
            CinemachineBrain = Camera.GetComponent<CinemachineBrain>();
        }
    }
}
