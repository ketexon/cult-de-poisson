using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LockCursor : MonoBehaviour
{
    static Stack<CursorLockMode> lockModeStack = new();
    public static void PushLockState(CursorLockMode lm)
    {
        lockModeStack.Push(Cursor.lockState);
        Cursor.lockState = lm;
    }

    public static void PopLockState()
    {
        Cursor.lockState = lockModeStack.Pop();
    }

    public void OnActivate(InputAction.CallbackContext ctx)
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnCancel(InputAction.CallbackContext ctx)
    {
        Cursor.lockState = CursorLockMode.None;
    }
}
