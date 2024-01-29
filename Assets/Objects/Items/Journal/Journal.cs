using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class Journal : Item
{
    [SerializeField]
    CinemachineVirtualCamera virtualCamera;

    [SerializeField]
    InputActionReference exitAction;

    bool usingJournal = false;

	private void Start()
	{
		Canvas canvas = GetComponentInChildren<Canvas>();
        canvas.worldCamera = mainCamera;
	}

	public override void Initialize(InitializeParams initParams)
    {
        base.Initialize(initParams);

        exitAction.action.performed += StopUsingJournal;
    }

    public override void OnUse()
    {
        base.OnUse();

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

    void StopUsingJournal(CallbackContext ctx)
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
    }
}
