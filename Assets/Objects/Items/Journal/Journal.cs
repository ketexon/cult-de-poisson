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

    Animator animator;

    bool usingJournal = false;
    bool inAnimation = false;

    int curPage = 0;

    private void Start()
    {
        Canvas canvas = GetComponentInChildren<Canvas>();
        canvas.worldCamera = mainCamera;

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
        curPage = pageNumber;

        foreach (GameObject page in journalPages)
        {
            page.SetActive(false);
        }

        yield return StartCoroutine(TriggerAnimation(TURN_PAGE_TRIGGER));

        journalPages[pageNumber].SetActive(true);
    }

    IEnumerator OpenJournalInternal()
    {
        inAnimation = true;

        yield return TriggerAnimation(OPEN_JOURNAL_TRIGGER);

        journalPages[0].SetActive(true);
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
