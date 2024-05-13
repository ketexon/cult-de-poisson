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
        public int Order = 0;
    }

    [SerializeField] UIDocument document;
    [SerializeField] VisualTreeAsset interactionTemplate;
    [SerializeField] VisualTreeAsset keybindIconTemplate;

    VisualElement interactionContainer;
    VisualElement crosshair;

    VisualElement root => document.rootVisualElement;

    public bool CrosshairEnabled { get; private set; } = false;
    public bool CrosshairVisible { get; private set; } = true;

    readonly Dictionary<string, string> DisplayStringIconClassMap = new()
    {
        { "RMB", "keybind-icon--rmb" },
        { "LMB", "keybind-icon--lmb" },
        { "Scroll Wheel", "keybind-icon--scroll-y" },
    };

    override protected void Awake()
    {
        base.Awake();

        interactionContainer = root.Q<VisualElement>("interaction-container");
        crosshair = root.Q<VisualElement>(null, "crosshair");
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
            crosshair.EnableInClassList("crosshair--enabled", !CrosshairEnabled);
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
            crosshair.EnableInClassList("crosshair--invisible", !CrosshairVisible);
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
    public System.Action AddInputUI(InputAction inputAction, string message, bool disabled = false, int order = 0)
    {
        var ve = AddInputUIToDocument(new Entry
        {
            InputAction = inputAction,
            Message = message,
            Disabled = disabled,
            Order = order,
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
        var root = ve.Q<OrderableElement>("interaction-indicator");
        root.EnableInClassList("disabled", entry.Disabled);
        
        var iconsContainer = root.Q<VisualElement>(null, "interaction-indicator__icons");
        iconsContainer.Clear();

        var bindingDisplayStrings = InputUtil.GetActionDisplayStrings(entry.InputAction);
        foreach (var displayString in bindingDisplayStrings)
        {
            var icon = keybindIconTemplate.Instantiate();

            if (DisplayStringIconClassMap.ContainsKey(displayString))
            {
                icon.AddToClassList("keybind-icon--image");
                icon.AddToClassList(DisplayStringIconClassMap[displayString]);
            }
            else
            {
                icon.AddToClassList("keybind-icon--text");

                var iconLabel = icon.Q<Label>(null, "keybind-icon__text");
                iconLabel.text = displayString;
            }

            iconsContainer.Add(icon);
        }

        var label = ve.Q<Label>(null, "interaction-indicator__label");
        label.text = entry.Message;

        int index = 0;
        foreach(var child in interactionContainer.Children())
        {
            var orderable = child.Q<OrderableElement>("interaction-indicator");
            if (orderable.Order < entry.Order) break;
            ++index;
        }
        interactionContainer.Insert(index, ve);

        return ve;
    }
}
