using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Yarn.Unity;

public class DialogueBox : DialogueViewBase
{
    [SerializeField] TMP_Text textComponent;

    Player player;
    float textSpeed = 0.05f;
    string npcName;

    void Awake()
    {
        player = Player.Instance;
        npcName = GetComponentInParent<NPC>().npcSO.Name;
    }

    void OnEnable()
    {
        LookAtPlayerImmediate();
    }

    void Update()
    {
        LookAtPlayer();
    }

    /// <summary>
    /// Does a slight rotation towards the player
    /// </summary>
    void LookAtPlayer()
    {
        Vector3 playerTarget = new(player.transform.position.x, transform.position.y, player.transform.position.z);

        Quaternion target = Quaternion.LookRotation(playerTarget - transform.position);
        Quaternion current = transform.parent.rotation;

        float angle = Mathf.Max(Quaternion.Angle(current, target), 15f);

        transform.parent.rotation = Quaternion.RotateTowards(current, target, angle * Time.deltaTime);
    }

    /// <summary>
    /// Does an immediate rotation towards the player
    /// </summary>
    void LookAtPlayerImmediate()
    {
        Vector3 playerTarget = new(player.transform.position.x, transform.position.y, player.transform.position.z);

        Quaternion target = Quaternion.LookRotation(playerTarget - transform.position);
        transform.parent.rotation = target;
    }

    IEnumerator RunLineInternal(string text, Action onDialogueLineFinished)
    {
        foreach (char c in text)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        yield return new WaitForSeconds(3f);

        onDialogueLineFinished?.Invoke();
    }

    public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
    {
        if (dialogueLine.CharacterName != npcName)
        {
            onDialogueLineFinished?.Invoke();
            return;
        }

        textSpeed = 0.05f;
        StartCoroutine(RunLineInternal(dialogueLine.TextWithoutCharacterName.Text, onDialogueLineFinished));
    }

    public override void InterruptLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
    {
        textSpeed = 0.01f;
    }

    public override void DismissLine(Action onDismissalComplete)
    {
        textComponent.text = "";
        onDismissalComplete?.Invoke();
    }
}
