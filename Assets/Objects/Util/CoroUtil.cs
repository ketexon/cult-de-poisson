using System.Collections;
using UnityEngine;

public static class CoroUtil
{
    public static Coroutine WaitOneFrame(this MonoBehaviour behaviour, System.Action action)
    {
        IEnumerator Impl()
        {
            yield return null;
            action();
        }
        return behaviour.StartCoroutine(Impl());
    }
}