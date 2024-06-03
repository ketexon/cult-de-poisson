using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class EndScreen : MonoBehaviour
{
    [SerializeField] List<QuestSO> requiredQuests;
    [SerializeField] UIDocument doc;

    VisualElement root;

    HashSet<QuestSO> remainingQuests;

    void Awake()
    {
        remainingQuests = new(requiredQuests);

        QuestStateSO.Instance.CompletedEvent += OnQuestCompleted;

        root = doc.rootVisualElement.Q("EndScreen");
    }

    void OnQuestCompleted(QuestSO q)
    {
        remainingQuests.Remove(q);
        if(remainingQuests.Count == 0)
        {
            GoToEndGame();
        }
    }

    void GoToEndGame()
    {
        Debug.Log("ENDGAME");

        root.AddToClassList("end-screen--active");
        Player.Instance.PushActionMap("UI");
        LockCursor.PushLockState(CursorLockMode.None);

        EventSystem.current.SetSelectedGameObject(gameObject);
        var b = root.Q<Button>();
        b.Focus();

        UnityEngine.Cursor.lockState = CursorLockMode.None;

        void OnButtonClicked()
        {
            root.RemoveFromClassList("end-screen--active");

            Player.Instance.PopActionMap();
            LockCursor.PopLockState();

            b.clicked -= OnButtonClicked;
        }
        b.clicked += OnButtonClicked;

    }
}
