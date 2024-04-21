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

    [SerializeField] InputActionReference escapeAction;

    Stack<GameObject> panels;

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
    }

    void CloseMenu()
    {
        canvas.enabled = false;
        player.Input.ActivateInput();
        player.Camera.enabled = true;
    }

    public void PushPanel(GameObject panel)
    {
        if(panels.TryPeek(out var oldPanel)){
            oldPanel.SetActive(false);
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
        if(panels.Count > 0)
        {
            panels.Pop();
        }
        if(panels.Count == 0)
        {
            CloseMenu();
        }
    }
}
