using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputUtil
{
    static readonly HashSet<string> OutputtedDeviceLayouts = new()
    {
        "Mouse"
    };

    static readonly HashSet<string> AmbiguousBindings = new()
    {
        "Delta"
    };

    public static InputBinding? FindEffectiveBindingMask(InputAction action)
    {
        if (action.bindingMask.HasValue)
            return action.bindingMask;

        if (action.actionMap?.bindingMask != null)
            return action.actionMap?.bindingMask;

        return action.actionMap?.asset?.bindingMask;
    }

    public static List<string> GetActionDisplayStrings(InputAction action)
    {
        InputBinding.DisplayStringOptions options = default;

        InputBinding bindingMask = FindEffectiveBindingMask(action) ?? default;

        List<string> results = new();
        var bindings = action.bindings;
        for (var i = 0; i < bindings.Count; ++i)
        {
            if (!bindingMask.Matches(bindings[i]))
                continue;

            var text = action.GetBindingDisplayString(
                i,
                out var deviceLayoutName,
                out var _,
                options
            );

            if (OutputtedDeviceLayouts.Contains(deviceLayoutName)
                && AmbiguousBindings.Contains(text)
            )
            {
                results.Add($"{deviceLayoutName} {text}");
            }
            else
            {
                results.Add(text);
            }
            if (bindings[i].isPartOfComposite)
            {
                continue;
            }
            break;
        }

        return results;
    }
}