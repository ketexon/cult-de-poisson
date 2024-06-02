using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseRaycastTester : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        //Check what the mouse is hovering over
        EventSystem eventSystem = EventSystem.current;
        PointerEventData pointerEventData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerEventData, results);
        foreach (RaycastResult result in results)
        {
            Debug.Log(result.gameObject.name);
        }

    }
}
