using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SettingsUI : SingletonBehaviour<SettingsUI>
{
    [SerializeField] Canvas canvas;
    [SerializeField] Player player;
    [SerializeField] GameObject defaultPanel;

    [SerializeField] InputActionReference escapeAction;

    Stack<GameObject> panels = new();

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
            PopPanel();
        }
        else
        {
            OpenMenu();
        }
    }

    void OpenMenu()
    {
        player.Input.DeactivateInput();
        canvas.enabled = true;
        player.Camera.enabled = false;
        LockCursor.PushLockState(CursorLockMode.None);

        PushPanel(defaultPanel);
    }

    void CloseMenu()
    {
        canvas.enabled = false;
        player.Input.ActivateInput();
        player.Camera.enabled = true;
        LockCursor.PushLockState(CursorLockMode.Locked);
    }

    public void PushPanel(GameObject panel)
    {
        if(panels.TryPeek(out var lastPanel))
        {
            lastPanel.SetActive(false);
        }
        panels.Push(panel);
        panel.SetActive(true);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void PopPanel()
    {
        var top = panels.Pop();
        top.SetActive(false);
        if (panels.TryPeek(out var lastPanel))
        {
            lastPanel.SetActive(true);
        }
        else
        {
            CloseMenu();
        }
    }
}
