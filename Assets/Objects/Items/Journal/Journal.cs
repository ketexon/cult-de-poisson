using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class Journal : Item
{
    [SerializeField] CinemachineVirtualCamera virtualCamera;

	[SerializeField] InputActionReference exitAction;

	bool usingJournal = false;

	public override void Initialize(InitializeParams initParams)
	{
		base.Initialize(initParams);

		exitAction.action.performed += OnExitJournal;
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
		OnExitJournal(new());
		exitAction.action.performed -= OnExitJournal;
		base.OnStopUsingItem();
	}

	void OnExitJournal(CallbackContext ctx)
	{
		playerInput.SwitchCurrentActionMap("Gameplay");

		if (usingJournal)
		{
			LockCursor.PopLockState();
		}

		usingJournal = false;
		virtualCamera.enabled = false;
	}
}
