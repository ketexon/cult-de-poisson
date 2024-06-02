using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public abstract class JournalUIElement : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{

    protected Journal Journal { get; private set; }

    virtual protected void Awake()
    {
        Journal = GetComponentInParent<Journal>();
    }

    virtual protected void OnEnable()
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
