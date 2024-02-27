using Unity.VisualScripting;
using UnityEngine;
using Yarn.Unity;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] DialogueViewBase optionsView = null;

    DialogueRunner dialogueRunner;
    void Awake()
    {
        dialogueRunner = GetComponent<DialogueRunner>();
        dialogueRunner.dialogueViews = new DialogueViewBase[2];
        dialogueRunner.dialogueViews[1] = optionsView;
    }

    void OnEnable()
    {
        dialogueRunner.onDialogueComplete.AddListener(OnEndDialogue);
    }

    void OnDisable()
    {
        dialogueRunner.onDialogueComplete.RemoveListener(OnEndDialogue);
    }

    //TODO: Check for already running dialogue
    public void StartDialogue(NPC npc, string startNode)
    {
        NPCSO so = npc.npcSO;
        npc.DialogueView.gameObject.SetActive(true);
        dialogueRunner.dialogueViews[0] = npc.DialogueView;
        dialogueRunner.StartDialogue(startNode);
    }

    void OnEndDialogue()
    {
        dialogueRunner.dialogueViews[0].gameObject.SetActive(false);
        dialogueRunner.dialogueViews[0] = null;
    }
}
