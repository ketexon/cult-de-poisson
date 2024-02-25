using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsUI : SingletonBehaviour<SettingsUI>
{
    [SerializeField] Canvas canvas;
    [SerializeField] PlayerInput playerInput;
    [SerializeField] Camera playerCamera;

    [SerializeField] InputActionReference escapeAction;

    bool Open => canvas.enabled;

    override protected void Awake()
    {
        escapeAction.action.actionMap.Enable();

        escapeAction.action.performed += OnInputEscape;

        if (Open)
        {
            CloseMenu();
        }
    }

    void OnDestroy()
    {
        escapeAction.action.performed -= OnInputEscape;
    }

    void OnInputEscape(InputAction.CallbackContext ctx)
    {
        if (Open)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    void OpenMenu()
    {
        
        playerInput.DeactivateInput();
        canvas.enabled = true;
    }

    void CloseMenu()
    {
        canvas.enabled = false;
        playerInput.ActivateInput();
    }
}
