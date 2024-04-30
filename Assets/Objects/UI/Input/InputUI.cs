using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class InputUI : SingletonBehaviour<InputUI>
{
    [SerializeField] Animator crosshairAnimator;

    [SerializeField] UIDocument document;
    [SerializeField] VisualTreeAsset interactionTemplate;

    VisualElement interactionContainer;

    public bool CrosshairEnabled { get; private set; } = false;
    public bool CrosshairVisible { get; private set; } = true;

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
        if(CrosshairEnabled != value)
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
        var ve = interactionTemplate.Instantiate();
        if (disabled)
        {
            ve.Q<VisualElement>("interact-indicator").AddToClassList("disabled");
        }
        ve.Q<Label>(null, "button__key").text = inputAction.GetBindingDisplayString();
        ve.Q<Label>(null, "button__label").text = message;

        interactionContainer.Add(ve);

        return () =>
        {
            ve.RemoveFromHierarchy();
        };
    }
}
