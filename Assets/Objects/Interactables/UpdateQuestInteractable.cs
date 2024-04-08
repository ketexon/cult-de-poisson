using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateQuestInteractable : Interactable
{
    [SerializeField] QuestSO quest;
    [SerializeField] QuestStatus targetStatus;
    [SerializeField] string message;

    public override string InteractMessage => string.Format(message, quest.Name);

    void Awake()
    {
        QuestStateSO.Instance.StartedEvent += OnQuestChanged;
        QuestStateSO.Instance.CancelledEvent += OnQuestChanged;
        QuestStateSO.Instance.CompletedEvent += OnQuestChanged;

        UpdateEnabled();
    }

    void Start()
    {}

    void OnDestroy()
    {
        QuestStateSO.Instance.StartedEvent -= OnQuestChanged;
        QuestStateSO.Instance.CancelledEvent -= OnQuestChanged;
        QuestStateSO.Instance.CompletedEvent -= OnQuestChanged;
    }

    void OnQuestChanged(QuestSO quest)
    {
        // only update if this quest has prerequesites
        // or the updated quest is this quest
        if(quest == this.quest || this.quest.Subquests.Count > 0)
        {
            UpdateEnabled();
        }
    }

    public override void OnInteract()
    {
        base.OnInteract();

        QuestStateSO.Instance.CompleteQuest(quest);
    }

    // Update whether this quest is interactable
    // Takes into account the target state (eg. can't complete a quest
    // that you haven't started/without prereqs)
    void UpdateEnabled()
    {
        var status = QuestStateSO.Instance.GetStatus(quest);
        if (targetStatus == QuestStatus.InProgress)
        {
            // we can only start a quest that is not started
            enabled = status == QuestStatus.NotStarted;
        }
        else if (targetStatus == QuestStatus.Completed)
        {
            // we only see the quest if it is inprogress (unless MustStartToComplete is false)
            enabled = status == QuestStatus.InProgress
                || (!quest.MustStartToComplete && status == QuestStatus.NotStarted);

            // we can only interact if we can complete the quest
            CanInteract = QuestStateSO.Instance.CanComplete(quest);
        }
    }
}
