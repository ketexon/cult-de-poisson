using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Class that contains references to all player behaviours
/// </summary>
public class Player : MonoBehaviour
{
    public PlayerInput Input;
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
