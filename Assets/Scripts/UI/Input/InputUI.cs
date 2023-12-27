using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputUI : Singleton<InputUI>
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

    public System.Action AddInputUI(InputAction inputAction, string message)
    {
        var go = Instantiate(inputUIElementPrefab, inputUIElementParent.transform);
        go.GetComponent<InputUIElement>().Initialize(
            inputAction.GetBindingDisplayString(),
            message
        );
        
        return () =>
        {
            Destroy(go);
        };
    }
}
