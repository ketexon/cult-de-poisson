using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.Rendering;

[RequireComponent(typeof(Button))]
public class FishImageButton : JournalUIElement
{
    readonly Color higlight = new(0.5f, 1, 0.5f, 1);

    [SerializeField]
    SpeciesDropdown dropdown;
    [SerializeField]
    FishSO fish;

    Image spriteRenderer = null;
    Button button = null;

    bool isInteractable = false;

    public override void Reset()
    {
        if (!spriteRenderer) { spriteRenderer = GetComponent<Image>(); }
        if (!button)
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(() => dropdown.SetFishSO(fish));
        }

        isInteractable = fish && Journal.GetUnlockedFishNames().Contains(fish.Name);
        button.interactable = isInteractable;
    }
}
