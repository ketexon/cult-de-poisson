using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.EventSystems;
using System.Linq;

public class Journal : Item
{
    const string TURN_PAGE_FORWARD_TRIGGER = "turnPageForward";
    const string TURN_PAGE_BACKWARD_TRIGGER = "turnPageBack";
    const string OPEN_JOURNAL_TRIGGER = "open";
    const string CLOSE_JOURNAL_TRIGGER = "close";


    [SerializeField]
    CinemachineVirtualCamera virtualCamera;

    [SerializeField]
    InputActionReference exitAction, turnPageAction;

    [SerializeField]
    GameObject[] journalHeadPages = new GameObject[0];

    [SerializeField]
    Bookmark[] bookmarks = new Bookmark[3];

    Animator animator;
    Action inputUIDestructor = null;
    List<InputUI.Entry> inputUIEntries = new();

    bool inAnimation = false;
    bool usingJournal = false;
    int curSection = 0;

    public override string TargetInteractMessage => "to open journal";
    public override bool TargetInteractVisible => !usingJournal;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();

        OnExitJournal();

        foreach (GameObject page in journalHeadPages)
        {
            if (page.TryGetComponent<Canvas>(out var canvas))
            {
                canvas.worldCamera = mainCamera;
            }
        }

        InitializeInputUI();
    }

    public override void Initialize(InitializeParams initParams)
    {
        base.Initialize(initParams);

        exitAction.action.performed += StopUsingJournal;
        turnPageAction.action.performed += TurnPage;
    }

    public override void OnInteract()
    {
        if (journalHeadPages.Length == 0)
        {
            Debug.LogError("Journal has no pages to display.");
            return;
        }

        usingJournal = true;
        playerInput.SwitchCurrentActionMap("Journal");
        virtualCamera.enabled = true;
        LockCursor.PushLockState(CursorLockMode.None);

        foreach (Bookmark bookmark in bookmarks)
        {
            bookmark.gameObject.SetActive(true);
        }

        inputUIDestructor = InputUI.Instance.AddInputUI(inputUIEntries);

        StartCoroutine(OpenJournalInternal());
    }

    public override void OnStopUsingItem()
    {
        OnExitJournal();
        exitAction.action.performed -= StopUsingJournal;
        turnPageAction.action.performed -= TurnPage;
        base.OnStopUsingItem();
    }

    public void OpenPage(int pageNumber)
    {
        if (pageNumber < 0 || pageNumber >= journalHeadPages.Length)
        {
            return;
        }

        if (pageNumber == curSection || inAnimation)
        {
            return;
        }

        StartCoroutine(OpenPageInternal(pageNumber));
    }
    void StopUsingJournal(CallbackContext ctx) //Exiting the journal
    {
        playerInput.SwitchCurrentActionMap("Gameplay");

        StartCoroutine(TriggerAnimation(CLOSE_JOURNAL_TRIGGER));

        foreach (Bookmark bookmark in bookmarks)
        {
            bookmark.gameObject.SetActive(false);
        }

        inputUIDestructor?.Invoke();
        OnExitJournal();
    }

    void TurnPage(CallbackContext ctx)
    {
        if (ctx.performed && usingJournal)
        {
            if (ctx.ReadValue<float>() > 0)
            {
                OpenPage(curSection + 1);
            }
            else
            {
                OpenPage(curSection - 1);
            }
        }
    }

    void OnExitJournal()
    {
        if (usingJournal)
        {
            LockCursor.PopLockState();
        }

        usingJournal = false;
        virtualCamera.enabled = false;

        foreach (GameObject page in journalHeadPages)
        {
            page.SetActive(false);
        }
    }

    IEnumerator OpenPageInternal(int pageNumber)
    {
        foreach (GameObject page in journalHeadPages)
        {
            page.SetActive(false);
        }

        if (pageNumber > curSection)
        {
            yield return StartCoroutine(TriggerAnimation(TURN_PAGE_FORWARD_TRIGGER));
        }
        else
        {
            yield return StartCoroutine(TriggerAnimation(TURN_PAGE_BACKWARD_TRIGGER));
        }

        InitializePage(pageNumber);
    }

    private void InitializePage(int pageNumber)
    {
        GameObject newPage = journalHeadPages[pageNumber];

        curSection = pageNumber;
        newPage.SetActive(true);
        newPage.GetComponent<Canvas>().worldCamera = mainCamera;

        Button[] buttons = newPage.GetComponentsInChildren<Button>();
        if (buttons.Length > 0)
        {
            EventSystem.current.SetSelectedGameObject(buttons.Where(b => b.interactable).First().gameObject);
        }
    }

    IEnumerator OpenJournalInternal()
    {
        inAnimation = true;

        yield return TriggerAnimation(OPEN_JOURNAL_TRIGGER);

        journalHeadPages[0].SetActive(true);
        journalHeadPages[0].GetComponent<Canvas>().worldCamera = mainCamera;

        InitializePage(0);
    }

    IEnumerator TriggerAnimation(string animationTrigger)
    {
        inAnimation = true;
        animator.SetTrigger(animationTrigger);

        yield return null;
        AnimatorClipInfo[] info;


        info = animator.GetNextAnimatorClipInfo(0);

        if (info.Length == 0)
        {
            info = animator.GetCurrentAnimatorClipInfo(0);
        }

        yield return new WaitForSeconds(info[0].clip.length);
        inAnimation = false;
    }

    public string[] GetUnlockedFish()
    {
        return new string[] { "Key Fish", "Flounder" };
    }


    void InitializeInputUI()
    {
        inputUIEntries.Add(new InputUI.Entry
        {
            Message = $"to close the journal",
            InputAction = exitAction,
        });
        inputUIEntries.Add(new InputUI.Entry
        {
            Message = $"to turn the page",
            InputAction = turnPageAction,
        });
    }
}
