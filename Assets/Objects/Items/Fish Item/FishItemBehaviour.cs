using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FishItemBehaviour : MonoBehaviour, IInteractItem
{
    public virtual bool InteractsWithInteractable => false;

    public virtual bool InteractVisible => false;

    public virtual bool InteractEnabled => false;

    public virtual string InteractMessage => null;

    public virtual void OnInteract(Interactable target)
    {}
}