using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPC : Interactable
{
    //TODO: Integrate animations with Yarnspinner
    //TODO: Pathfinding

    public NPCSO npcSO;
    public DialogueBox DialogueView;

    DialogueManager dialogueManager;

    void Start()
    {
        dialogueManager = Player.Instance.DialogueManager;
        DialogueView.gameObject.SetActive(false);
    }

    public override string TargetInteractMessage => "Talk to " + npcSO.Name;

    public override void OnInteract()
    {
        dialogueManager.StartDialogue(this, npcSO.YarnStartNode);
    }

}
