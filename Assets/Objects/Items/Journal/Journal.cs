using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;
using static UnityEngine.InputSystem.InputAction;

public class Journal : Item
{
    const string TURN_PAGE_TRIGGER = "turnPage";
    const string OPEN_JOURNAL_TRIGGER = "open";
    const string CLOSE_JOURNAL_TRIGGER = "close";


    [SerializeField]
    CinemachineVirtualCamera virtualCamera;

    [SerializeField]
    InputActionReference exitAction;

    [SerializeField]
    GameObject[] journalPages = new GameObject[0];

    [SerializeField]
    Bookmark[] bookmarks = new Bookmark[3];

    Animator animator;

    bool inAnimation = false;
    bool usingJournal = false;
    int curPage = 0;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();

        OnExitJournal();
    }

    public override void Initialize(InitializeParams initParams)
    {
        base.Initialize(initParams);

        exitAction.action.performed += StopUsingJournal;
    }

    public override void OnUse()
    {
        if (journalPages.Length == 0)
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

        StartCoroutine(OpenJournalInternal());
    }

    public override void OnStopUsingItem()
    {
        OnExitJournal();
        exitAction.action.performed -= StopUsingJournal;
        base.OnStopUsingItem();
    }

    public void OpenPage(int pageNumber)
    {
        if (pageNumber < 0 || pageNumber >= journalPages.Length)
        {
            Debug.LogError("Invalid page number: " + pageNumber);
            return;
        }

        if (pageNumber == curPage || inAnimation)
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

        OnExitJournal();
    }

    void OnExitJournal()
    {
        if (usingJournal)
        {
            LockCursor.PopLockState();
        }

        usingJournal = false;
        virtualCamera.enabled = false;

        foreach (GameObject page in journalPages)
        {
            page.SetActive(false);
        }
    }

    IEnumerator OpenPageInternal(int pageNumber)
    {
        JournalUIElement[] journalUIElements = journalPages[curPage].GetComponentsInChildren<JournalUIElement>();

        foreach (JournalUIElement element in journalUIElements)
        {
            element.Reset();
        }

        curPage = pageNumber;

        foreach (GameObject page in journalPages)
        {
            page.SetActive(false);
        }

        yield return StartCoroutine(TriggerAnimation(TURN_PAGE_TRIGGER));

        journalPages[pageNumber].SetActive(true);
        journalPages[pageNumber].GetComponent<Canvas>().worldCamera = mainCamera;
    }

    IEnumerator OpenJournalInternal()
    {
        inAnimation = true;

        yield return TriggerAnimation(OPEN_JOURNAL_TRIGGER);

        journalPages[0].SetActive(true);
        journalPages[0].GetComponent<Canvas>().worldCamera = mainCamera;
    }

    IEnumerator TriggerAnimation(string animationTrigger)
    {
        inAnimation = true;
        animator.SetTrigger(animationTrigger);

        yield return null;
        AnimatorClipInfo info = animator.GetNextAnimatorClipInfo(0)[0];

        yield return new WaitForSeconds(info.clip.length);
        inAnimation = false;
    }

}
