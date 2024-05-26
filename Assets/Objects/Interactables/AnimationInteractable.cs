using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationInteractable : Interactable
{
    [SerializeField] Animator animator;
    [SerializeField] string message;
    [SerializeField] string trigger;

    bool opened = false;

    public string Trigger => trigger;

    public override bool TargetInteractVisible => !opened;
    public override string TargetInteractMessage => message;

    public override void OnInteract()
    {
        animator.SetTrigger(trigger);
        opened = true;
    }
}
