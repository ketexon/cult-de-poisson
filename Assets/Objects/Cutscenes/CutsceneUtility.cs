using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneUtility : MonoBehaviour
{
    public void SetInputActivated(bool active)
    {
        if(active){
            Player.Instance.Input.ActivateInput();
        }
        else
        {
            Player.Instance.Input.DeactivateInput();
        }
    }

    public void TeleportPlayer(Transform target)
    {
        Player.Instance.Movement.Teleport(target);
    }

    public void StartDialogue(NPC npc, string name)
    {
        Player.Instance.DialogueManager.StartDialogue(npc, name);
    }
}
