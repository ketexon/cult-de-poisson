using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LockCursor : MonoBehaviour
{
    public void OnActivate(InputAction.CallbackContext ctx)
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnCancel(InputAction.CallbackContext ctx)
    {
        Cursor.lockState = CursorLockMode.None;
    }
}
