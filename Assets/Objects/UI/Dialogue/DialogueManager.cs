using Unity.VisualScripting;
using UnityEngine;
using Yarn.Unity;

public class DialogueManager : MonoBehaviour
{
    DialogueRunner dialogueRunner;
    bool dialogueRunning = false;
    void Awake()
    {
        dialogueRunner = GetComponent<DialogueRunner>();
    }

    void OnEnable()
    {
        dialogueRunner.onDialogueComplete.AddListener(OnEndDialogue);
        dialogueRunner.onNodeComplete.AddListener(OnNodeComplete);
    }

    void OnDisable()
    {
        dialogueRunner.onDialogueComplete.RemoveListener(OnEndDialogue);
        dialogueRunner.onNodeComplete.RemoveListener(OnNodeComplete);
    }

    public void StartDialogue(NPC npc, string startNode)
    {
        if (dialogueRunning) { return; }

        dialogueRunning = true;

        dialogueRunner.StartDialogue(startNode);
    }

    void OnEndDialogue()
    {
        dialogueRunning = false;
    }

    void OnNodeComplete(string input)
    {
        Debug.Log(input);
    }
}
