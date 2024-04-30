using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class InputUI : SingletonBehaviour<InputUI>
{
    public class Entry
    {
        public InputAction InputAction;
        public string Message;
        public bool Disabled = false;
    }

    [SerializeField] Animator crosshairAnimator;

    [SerializeField] UIDocument document;
    [SerializeField] VisualTreeAsset interactionTemplate;

    VisualElement interactionContainer;

    public bool CrosshairEnabled { get; private set; } = false;
    public bool CrosshairVisible { get; private set; } = true;

    readonly Dictionary<string, string> DisplayStringIconClassMap = new()
    {
        { "RMB", "icon--rmb" },
        { "LMB", "icon--lmb" },
        { "Scroll y", "icon--scroll-y" },
    };

    override protected void Awake()
    {
        base.Awake();

        interactionContainer = document.rootVisualElement.Q<VisualElement>("interaction-container");
        interactionContainer.Clear();
    }

    /// <summary>
    /// Set the crosshair enabled state.
    /// If the crosshair is enabled, its opacity is increased.
    /// </summary>
    /// <param name="value"></param>
    public void SetCrosshairEnabled(bool value)
    {
        if (CrosshairEnabled != value)
        {
            CrosshairEnabled = value;
            crosshairAnimator.SetBool("Enabled", CrosshairEnabled);
        }
    }

    /// <summary>
    /// Set the crosshair visibility state.
    /// Invisible will not show any crosshair, and visible will show the crosshair
    /// with transparency corresponding to the enabled state.
    /// </summary>
    /// <param name="value"></param>
    public void SetCrosshairVisible(bool value)
    {
        if (CrosshairVisible != value)
        {
            CrosshairVisible = value;
            crosshairAnimator.SetBool("Visible", CrosshairVisible);
        }
    }

    /// <summary>
    /// Add UI to the screen indicating an input that can be performed.
    /// Use the return value to remove the UI from the screen
    /// </summary>
    /// <param name="inputAction">The input action corresponding to the input</param>
    /// <param name="message">The message to display next to the input action</param>
    /// <param name="disabled">Whether the message should show as disabled (grey font)</param>
    /// <returns>A callback to call to remove the input.</returns>
    public System.Action AddInputUI(InputAction inputAction, string message, bool disabled = false)
    {
        var ve = AddInputUIToDocument(new Entry
        {
            InputAction = inputAction,
            Message = message,
            Disabled = disabled
        });

        return () =>
        {
            ve.RemoveFromHierarchy();
        };
    }

    public System.Action AddInputUI(IEnumerable<Entry> entries)
    {
        List<VisualElement> ves = new List<VisualElement>();
        foreach(var entry in entries)
        {
            ves.Add(AddInputUIToDocument(entry));
        }
        return () =>
        {
            foreach(var ve in ves)
            {
                ve.RemoveFromHierarchy();
            }
        };
    }

    private VisualElement AddInputUIToDocument(Entry entry)
    {
        var ve = interactionTemplate.Instantiate();
        var root = ve.Q<VisualElement>("interaction-indicator");
        if (entry.Disabled)
        {
            root.AddToClassList("disabled");
        }
        string bindingDisplayString = InputUtil.GetActionDisplayString(entry.InputAction);
        if (DisplayStringIconClassMap.ContainsKey(bindingDisplayString))
        {
            root.AddToClassList("button--image-icon");

            var imageIcon = ve.Q<VisualElement>(null, "button__image-icon");
            imageIcon.AddToClassList(DisplayStringIconClassMap[bindingDisplayString]);
        }
        else
        {
            root.AddToClassList("button--text-icon");

            var textIcon = ve.Q<Label>(null, "button__text-icon");
            textIcon.text = bindingDisplayString;
        }

        var label = ve.Q<Label>(null, "button__label");
        label.text = entry.Message;

        interactionContainer.Add(ve);

        return ve;
    }
}
