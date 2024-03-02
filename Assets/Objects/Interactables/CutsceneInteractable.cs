using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneInteractable : Interactable
{
    [SerializeField] string message;
    [SerializeField] PlayableDirector director;

    public override string InteractMessage => message;

    public override void OnInteract()
    {
        base.OnInteract();

        director.Play();
    }
}
