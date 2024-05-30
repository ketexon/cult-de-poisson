using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

// Eel Interactable, can be interacted with if holding any eel fish
// Interact not implemented yet
public class EelInteractable : Interactable
{
    [SerializeField] public QuestSO Quest;

    public override bool TargetInteractVisible => false;
    public override string TargetInteractMessage => null;

    public override void OnInteract()
    {
    }
}
