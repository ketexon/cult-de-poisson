using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.Serialization.Json;
using UnityEngine;

[System.Serializable]
public enum QuestStatus
{
    NotStarted,
    InProgress,
    Completed,
};

[CreateAssetMenu(fileName = "QuestState", menuName = "State/Quest State")]
public class QuestStateSO : SingletonObject<QuestStateSO>
{
    [SerializeField]
    List<QuestSO> defaultCompleted = new();

    [SerializeField]
    List<QuestSO> defaultInProgress = new();

    public System.Action<QuestSO> StartedEvent;
    public System.Action<QuestSO> CancelledEvent;
    public System.Action<QuestSO> CompletedEvent;

    Dictionary<QuestSO, QuestStatus> questStatus = new();

    override protected void OnEnable()
    {
        base.OnEnable();

        foreach(var quest in defaultInProgress)
        {
            questStatus[quest] = QuestStatus.InProgress;
        }

        foreach (var quest in defaultCompleted)
        {
            questStatus[quest] = QuestStatus.Completed;
        }
    }

    public void StartQuest(QuestSO quest)
    {
        questStatus[quest] = QuestStatus.InProgress;
        StartedEvent?.Invoke(quest);
    }

    public void CancelQuest(QuestSO quest)
    {
        questStatus.Remove(quest);
        CancelledEvent?.Invoke(quest);
    }

    public void CompleteQuest(QuestSO quest)
    {
        questStatus[quest] = QuestStatus.Completed;
        CompletedEvent?.Invoke(quest);
    }

    public QuestStatus GetStatus(QuestSO quest)
    {
        return questStatus.GetValueOrDefault(quest, QuestStatus.NotStarted);
    }

    /// <summary>
    /// Whether the quest has all its prerequesites completed (Subquests)
    /// </summary>
    /// <param name="quest"></param>
    /// <returns></returns>
    public bool CanComplete(QuestSO quest)
    {
        foreach(var subq in quest.Subquests)
        {
            if (GetStatus(subq) != QuestStatus.Completed) return false;
        }
        if (quest.MustStartToComplete && GetStatus(quest) != QuestStatus.InProgress) return false;
        return true;
    }
}
