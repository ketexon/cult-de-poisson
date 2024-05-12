using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

class PanelSwitchButton : Button
{
    public new class UxmlFactory : UxmlFactory<PanelSwitchButton, UxmlTraits> { }

    public new class UxmlTraits : Button.UxmlTraits
    {
        UxmlStringAttributeDescription targetPanel =
            new UxmlStringAttributeDescription { name = "target-panel", defaultValue = "" };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var button = ve as PanelSwitchButton;

            button.TargetPanel = targetPanel.GetValueFromBag(bag, cc);
        }
    }

    public string TargetPanel { get; set; }
}