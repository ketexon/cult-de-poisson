using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class JournalUIElement : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{

    protected Journal Journal { get; private set; }

    void Start()
    {
        Journal = GetComponentInParent<Journal>();
    }

    public virtual void OnPointerClick(PointerEventData eventData) { }

    public virtual void OnPointerDown(PointerEventData eventData) { }

    public virtual void OnPointerUp(PointerEventData eventData) { }

    public virtual void Reset() { }

}
