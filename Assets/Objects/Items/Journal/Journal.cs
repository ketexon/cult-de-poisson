using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;
using static UnityEngine.InputSystem.InputAction;

public class Journal : Item
{
    const string TURN_PAGE_TRIGGER = "turnPage";

    [SerializeField]
    CinemachineVirtualCamera virtualCamera;

    [SerializeField]
    InputActionReference exitAction;

    [SerializeField]
    GameObject[] journalPages = new GameObject[0];

    Animator animator;

    bool usingJournal = false;
    bool turningPage = false;

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

        journalPages[0].SetActive(true);

        usingJournal = true;
        playerInput.SwitchCurrentActionMap("Journal");
        virtualCamera.enabled = true;
        LockCursor.PushLockState(CursorLockMode.None);
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

        if (pageNumber == curPage)
        {
            return;
        }

        StartCoroutine(OpenPageInternal(pageNumber));
    }
    void StopUsingJournal(CallbackContext ctx) //Exiting the journal
    {
        playerInput.SwitchCurrentActionMap("Gameplay");

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
        turningPage = true;
        curPage = pageNumber;

        foreach (GameObject page in journalPages)
        {
            page.SetActive(false);
        }

        animator.SetTrigger(TURN_PAGE_TRIGGER);

        yield return null; //wait for a frame so the animator clip has a chance to be set
        AnimatorClipInfo info = animator.GetCurrentAnimatorClipInfo(0)[0];

        yield return new WaitForSeconds(info.clip.length);

        journalPages[pageNumber].SetActive(true);
        turningPage = false;
    }

    public void SetTurningPage(bool turningPage)
    {
        this.turningPage = turningPage;
    }

}
