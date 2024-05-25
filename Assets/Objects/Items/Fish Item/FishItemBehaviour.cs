using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FishItemBehaviour : MonoBehaviour, IInteractAgent, IInteractTarget
{
    public virtual bool TargetInteractVisible => false;
    public virtual bool TargetInteractEnabled => true;
    public virtual string TargetInteractMessage => null;


    public virtual bool AgentInteractVisible(Interactable target) => false;
    public virtual string AgentInteractMessage(Interactable target) => null;
    public virtual bool AgentInteractEnabled(Interactable target) => true;

    public virtual System.Action<IInteractObject> InteractivityChangeEvent { get; set; }

    public virtual void OnInteract() { }
    public virtual void OnInteract(Interactable target) { }
}