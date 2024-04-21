using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This class provides static functions to lock/unlock the cursor (ie. make cursor visible/invisible)
/// </summary>
public class LockCursor : MonoBehaviour
{
    Player player;
    static Stack<CursorLockMode> lockModeStack = new();


    void Awake()
    {
        player = GetComponent<Player>();
    }

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
}
