using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

// Door, can be interacted with if holding any key fish
// Interact not implemented yet
public class DoorInteractable : Interactable
{
    /// <summary>
    /// cannot open door without a key, so the message will always 
    /// be "Locked" if the door is the *target*
    /// </summary>
    public override string TargetInteractMessage => "Door is Locked";

    public override void OnInteract()
    {
        Debug.Log("WHAT");
    }
}
