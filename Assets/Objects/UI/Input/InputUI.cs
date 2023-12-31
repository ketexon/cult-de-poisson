using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputUI : SingletonBehaviour<InputUI>
{
    [SerializeField] GameObject inputUIElementPrefab;
    [SerializeField] GameObject inputUIElementParent;

    void Reset()
    {
        var candidate = GetComponentInChildren<LayoutGroup>();
        if (candidate != null)
        {
            inputUIElementParent = candidate.gameObject;
        }
        else
        {
            inputUIElementParent = gameObject;
        }
    }

    override protected void Awake()
    {
        base.Awake();
        foreach(Transform t in transform)
        {
            Destroy(t.gameObject);
        }
    }

    public System.Action AddInputUI(InputAction inputAction, string message, bool disabled = false)
    {
        var go = Instantiate(inputUIElementPrefab, inputUIElementParent.transform);
        go.GetComponent<InputUIElement>().Initialize(
            inputAction.GetBindingDisplayString(),
            message,
            disabled
        );
        
        return () =>
        {
            Destroy(go);
        };
    }
}
