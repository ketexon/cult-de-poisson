using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class KeyFishItemBehaviour : FishItemBehaviour
{
    public override bool TargetInteractEnabled => true;
    public override bool TargetInteractVisible => true;
    public override string TargetInteractMessage => "Use Keyfish";

    public override bool AgentInteractVisible(Interactable interactable) => interactable is DoorInteractable;
    public override string AgentInteractMessage(Interactable _) => "Open door";

    public override void OnInteract(Interactable target)
    {
        Debug.Log("Interact");
    }

    public override void OnInteract()
    {
        Debug.Log("Agent Interact");
    }
}