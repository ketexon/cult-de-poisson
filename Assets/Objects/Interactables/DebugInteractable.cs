using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugInteractable : Interactable
{
    public override string InteractMessage => "Debug Interactable";

    public override void OnInteract()
    {
        Debug.Log("Debug Interact");
        CanInteract = false;
        IEnumerator ResumeInteractableCoroutine()
        {
            yield return new WaitForSeconds(2.0f);
            CanInteract = true;
        }
        StartCoroutine(ResumeInteractableCoroutine());
    }
}
