using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class Journal : Item
{
    const string TURN_PAGE_FORWARD_TRIGGER = "turnPageForward";
    const string TURN_PAGE_BACKWARD_TRIGGER = "turnPageBack";
    const string OPEN_JOURNAL_TRIGGER = "open";
    const string CLOSE_JOURNAL_TRIGGER = "close";


    [SerializeField]
    CinemachineVirtualCamera virtualCamera;

    [SerializeField]
    InputActionReference exitAction;

    [SerializeField]
    GameObject[] journalHeadPages = new GameObject[0];

    [SerializeField]
    Bookmark[] bookmarks = new Bookmark[3];

    Animator animator;

    bool inAnimation = false;
    bool usingJournal = false;
    float curSection = 0;

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
        if (pageNumber < 0 || pageNumber >= journalHeadPages.Length)
        {
            Debug.LogError("Invalid page number: " + pageNumber);
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

        foreach (GameObject page in journalHeadPages)
        {
            page.SetActive(false);
        }
    }

    IEnumerator OpenPageInternal(int pageNumber)
    {
        JournalUIElement[] journalUIElements = journalHeadPages[(int)curSection].GetComponentsInChildren<JournalUIElement>();

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


        curSection = pageNumber;
        journalHeadPages[pageNumber].SetActive(true);
        journalHeadPages[pageNumber].GetComponent<Canvas>().worldCamera = mainCamera;
    }

    IEnumerator OpenJournalInternal()
    {
        inAnimation = true;

        yield return TriggerAnimation(OPEN_JOURNAL_TRIGGER);

        journalHeadPages[0].SetActive(true);
        journalHeadPages[0].GetComponent<Canvas>().worldCamera = mainCamera;
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
        return new string[] { "KeyFish", "TubeSnout" };
    }

}
