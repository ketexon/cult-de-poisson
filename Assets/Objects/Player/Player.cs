using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The player object. This is a singleton class. It can hold references to the various components of the Player object
/// </summary>
public class Player : SingletonBehaviour<Player>
{
    public PlayerInteract PlayerInteract;
    public DialogueManager DialogueManager;
}
