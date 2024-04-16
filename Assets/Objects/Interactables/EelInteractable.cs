using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

// Eel Interactable, can be interacted with if holding any eel fish
// Interact not implemented yet
public class EelInteractable : Interactable
{
    /// <summary>
    /// cannot open door without a key, so the message will always 
    /// be "Locked" if the door is the *target*
    /// </summary>
    public override string TargetInteractMessage => "Attach Eel to door to open it";

    public override void OnInteract()
    {
        Debug.Log("WHAT");
    }
}
