using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugInteractable : Interactable
{
    public override string TargetInteractMessage => "Debug Interactable";

    public override void OnInteract()
    {
        Debug.Log("Debug Interact");
        TargetInteractEnabled = false;
        IEnumerator ResumeInteractableCoroutine()
        {
            yield return new WaitForSeconds(2.0f);
            TargetInteractEnabled = true;
        }
        StartCoroutine(ResumeInteractableCoroutine());
    }
}
