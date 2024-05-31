using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class SettingsUI : SingletonBehaviour<SettingsUI>
{
    [SerializeField] UIDocument document;
    [SerializeField] Player player;
    [SerializeField] string defaultPanelName;

    [SerializeField] InputActionReference openSettingsAction;
    [SerializeField] InputActionReference escapeAction;

    Stack<VisualElement> panels = new();

    VisualElement root;
    bool Open => root.ClassListContains("settings--enabled");

    override protected void Awake()
    {
        openSettingsAction.action.performed += OnInputEscape;
        escapeAction.action.performed += OnInputEscape;

        root = document.rootVisualElement.Q<VisualElement>("settings");

        RegisterCallbacks();

        // if any other panels are active, disable them
        foreach (var ve in root.Query<VisualElement>().Class("settings-panel--active").Build())
        {
            ve.RemoveFromClassList("settings-panel--active");
        }

        if (Open)
        {
            root.EnableInClassList("settings--enabled", false);
        }
    }

    void OnDestroy()
    {
        escapeAction.action.performed -= OnInputEscape;
        openSettingsAction.action.performed -= OnInputEscape;
    }

    void RegisterCallbacks()
    {
        // register callbacks for PanelSwitchButtons
        root.Query<PanelSwitchButton>().ForEach(btn =>
        {
            if (btn.TargetPanel == null || btn.TargetPanel == string.Empty)
            {
                return;
            }
            btn.clicked += () =>
            {
                PushPanel(btn.TargetPanel);
            };
        });

        root.Q<Button>("settings__quit-button").clicked += () =>
        {
            Quit();
        };

        // fix scroll view from not changing sensitivity of moving using arrow keys
        root.Q<ScrollView>("settings__credits__entries").RegisterCallback<NavigationMoveEvent>((ev) =>
        {
            ev.StopPropagation();
            (ev.currentTarget as ScrollView).scrollOffset -= ev.move;
        }, TrickleDown.TrickleDown);

        // make a focus on the scrollview actually focus on the scrollbar
        root.Q<ScrollView>("settings__credits__entries").RegisterCallback<FocusEvent>((ev) =>
        {
            using var navEvent = NavigationMoveEvent.GetPooled(NavigationMoveEvent.Direction.Next);
            ev.currentTarget.SendEvent(navEvent);
        });
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
        //Time.timeScale = 0;

        root.EnableInClassList("settings--enabled", true);

        player.PushActionMap("UI");
        player.Camera.enabled = false;
        LockCursor.PushLockState(CursorLockMode.None);

        EventSystem.current.SetSelectedGameObject(gameObject);
        PushPanel(defaultPanelName);
    }

    void CloseMenu()
    {
        //Time.timeScale = 1;

        root.EnableInClassList("settings--enabled", false);

        player.PopActionMap();
        player.Camera.enabled = true;
        LockCursor.PopLockState();
    }

    public void PushPanel(string panelName)
    {
        if(panels.TryPeek(out var lastPanel))
        {
            lastPanel.EnableInClassList("settings-panel--active", false);
        }
        var ve = root.Q<VisualElement>(panelName);
        panels.Push(ve);
        ve.EnableInClassList("settings-panel--active", true);

        SelectFirstInPanel(ve);
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
        top.EnableInClassList("settings-panel--active", false);
        if (panels.TryPeek(out var lastPanel))
        {
            lastPanel.EnableInClassList("settings-panel--active", true);

            SelectFirstInPanel(lastPanel);
        }
        else
        {
            CloseMenu();
        }
    }

    void SelectFirstInPanel(VisualElement panel)
    {
        CoroUtil.WaitOneFrame(this, () => {
            var el = panel.Q<VisualElement>(null, "first-focus");
            if (el != null)
            {
                el.Focus();
            }
            else
            {
                using var navEvent = NavigationMoveEvent
                    .GetPooled(NavigationMoveEvent.Direction.Next);
                panel.SendEvent(navEvent);
            }
        });
    }
}
