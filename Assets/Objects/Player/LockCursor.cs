using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This class provides static functions to lock/unlock the cursor (ie. make cursor visible/invisible)
/// </summary>
public class LockCursor : MonoBehaviour
{
    static Stack<CursorLockMode> lockModeStack = new();

    /// <summary>
    /// Set the lock state for the cursor
    /// </summary>
    /// <param name="lm"></param>
    public static void PushLockState(CursorLockMode lm)
    {
        lockModeStack.Push(Cursor.lockState);
        Cursor.lockState = lm;
    }

    /// <summary>
    /// Revert the lock state for the cursor
    /// </summary>
    public static void PopLockState()
    {
        Cursor.lockState = lockModeStack.Pop();
    }

    /// <summary>
    /// Lock the cursor when you click on the game.
    /// </summary>
    /// <param name="ctx"></param>
    public void OnActivate(InputAction.CallbackContext ctx)
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Unlock the cursor when you press escape.
    /// </summary>
    /// <param name="ctx"></param>
    public void OnCancel(InputAction.CallbackContext ctx)
    {
        Cursor.lockState = CursorLockMode.None;
    }
}
