using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bookmark : JournalUIElement
{
    [SerializeField] string bookmarkName;
    [SerializeField] int pageNumber;

    public override void OnPointerClick(PointerEventData eventData)
    {
        Journal.OpenPage(pageNumber);
    }
}
