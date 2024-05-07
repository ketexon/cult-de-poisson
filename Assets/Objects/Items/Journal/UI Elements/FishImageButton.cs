using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class FishImageButton : JournalUIElement
{
    readonly Color higlight = new(0.5f, 1, 0.5f, 1);

    [SerializeField]
    SpeciesDropdown dropdown;
    [SerializeField]
    FishSO fish;

    Image spriteRenderer;
    bool isInteractable = true;

    void Start()
    {
        spriteRenderer = GetComponent<Image>();
        isInteractable = Journal.GetUnlockedFish().Contains(fish.name);
    }

    public override void Reset()
    {
        base.Reset();

        spriteRenderer.color = isInteractable ? Color.white : Color.gray;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (!isInteractable) return;

        spriteRenderer.color = higlight;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        if (!isInteractable) return;

        spriteRenderer.color = Color.white;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        if (!isInteractable) return;

        dropdown.SetFishSO(fish);
    }
}