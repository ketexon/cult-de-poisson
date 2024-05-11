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
    public PlayerItem Item;
    public PlayerInteract Interact;
    public PlayerMovement Movement;
    public Camera Camera;
    public CinemachineBrain CinemachineBrain;

    Stack<string> actionMaps = new();

    public void PushActionMap(string actionMap)
    {
        actionMaps.Push(actionMap);
        Input.SwitchCurrentActionMap(actionMap);
    }

    public void PopActionMap()
    {
        actionMaps.Pop();
        Input.SwitchCurrentActionMap(actionMaps.TryPeek(out var old) ? old : "Gameplay");
    }

    void Reset()
    {
        Input = GetComponentInChildren<PlayerInput>();
        Item = GetComponentInChildren<PlayerItem>();
        Interact = GetComponentInChildren<PlayerInteract>();
        Movement = GetComponentInChildren<PlayerMovement>();
        Camera = GetComponentInChildren<Camera>();
        if (Camera)
        {
            CinemachineBrain = Camera.GetComponent<CinemachineBrain>();
        }
    }
}
