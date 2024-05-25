using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

class OrderableElement : VisualElement
{
    public new class UxmlFactory : UxmlFactory<OrderableElement, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlIntAttributeDescription targetPanel =
            new () { name = "order", defaultValue = 0 };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var button = ve as OrderableElement;

            button.Order = targetPanel.GetValueFromBag(bag, cc);
        }
    }

    public int Order{ get; set; }
}