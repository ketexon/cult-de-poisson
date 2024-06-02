using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NotificationsUI : MonoBehaviour
{
    [SerializeField] UIDocument document;
    [SerializeField] VisualTreeAsset notificationTemplate;
    [SerializeField] float notificationDuration = 5.0f;
    [SerializeField] float notificationEnterDuration = 0.1f;
    [SerializeField] float notificationExitDuration = 0.1f;
    [SerializeField] Color fishColor;
    [SerializeField] QuestStateSO questState;

    VisualElement container;

    public enum Icon
    {
        None,
        Fishing,
        Journal,
        Satellite,
    }

    void Start()
    {
        container = document.rootVisualElement.Q<VisualElement>("notifications-container");

        container.Clear();

        questState.CompletedEvent += OnQuestCompleted;
    }

    void OnDestroy()
    {
        questState.CompletedEvent -= OnQuestCompleted;
    }

    void OnQuestCompleted(QuestSO questSO)
    {
        if(questSO.Type == QuestType.Satellite)
        {
            AddNotification($"satellite connected", Icon.Satellite);
        }
        else
        {
            AddNotification($"completed {questSO.Name.ToLower()}", Icon.Journal);
        }
    }

    public void AddCatchFishNotification(FishSO so)
    {
        AddCatchFishNotification(so.name.ToLower());
    }

    public void AddCatchFishNotification(string name)
    {
        AddNotification($"You caught a <b><color=#{fishColor.ToHex(includeAlpha: false)}>{name}</color></b>", Icon.Fishing);
    }

    public void AddJournalNotification()
    {
        AddNotification("new journal entry unlocked", Icon.Journal);
    }

    public VisualElement AddNotification(string message, Icon icon = Icon.None)
    {
        var notif = notificationTemplate.Instantiate();

        var label = notif.Q<Label>(null, "notification-indicator__label");
        label.text = message;

        if(icon != Icon.None)
        {
            notif.AddToClassList(icon switch
            {
                Icon.Fishing => "notification-indicator--fishing",
                Icon.Journal => "notification-indicator--journal",
                Icon.Satellite => "notification-indicator--satellite",
                _ => throw new NotImplementedException($"Icon {icon} not implemented")
            });
        }

        container.Add(notif);

        IEnumerator NotificationLifecycle()
        {
            notif.EnableInClassList("notification-indicator--enter", true);
            yield return null;
            notif.EnableInClassList("notification-indicator--enter-active", true);

            yield return new WaitForSeconds(notificationEnterDuration);

            notif.EnableInClassList("notification-indicator--enter", false);

            yield return new WaitForSeconds(notificationDuration);

            notif.EnableInClassList("notification-indicator--enter-active", false);
            notif.EnableInClassList("notification-indicator--exit", true);
            yield return null;
            notif.EnableInClassList("notification-indicator--exit-active", true);

            yield return new WaitForSeconds(notificationExitDuration);
            notif.RemoveFromHierarchy();
        }

        StartCoroutine(NotificationLifecycle());

        return notif;
    }
}
