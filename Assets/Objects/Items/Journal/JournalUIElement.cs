using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public abstract class JournalUIElement : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{

    protected Journal Journal { get; private set; }

    void Awake()
    {
        Journal = GetComponentInParent<Journal>();
    }

    void OnEnable()
    {
        Reset();
    }

    public virtual void OnPointerClick(PointerEventData eventData) { }

    public virtual void OnPointerDown(PointerEventData eventData) { }

    public virtual void OnPointerUp(PointerEventData eventData) { }

    public virtual void OnPointerEnter(PointerEventData eventData) { }

    public virtual void OnPointerExit(PointerEventData eventData) { }

    /// <summary>
    /// Resets the UI element to its default state.
    /// Called when a page is opened
    /// </summary>
    public virtual void Reset() { }

}
