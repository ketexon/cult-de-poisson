using System.Collections;
using UnityEngine;

public class SimpleHookedFish : HookedFish
{
    [SerializeField] float minTugInterval = 2;
    [SerializeField] float maxTugInterval = 3;

    [SerializeField] float minTugStrength = 1;
    [SerializeField] float maxTugStrength = 4;

    float TugInterval => Random.Range(minTugInterval, maxTugInterval);
    float TugStrength => Random.Range(minTugStrength, maxTugStrength);

    Coroutine coro = null;

    void Awake()
    {
        // just in case its not disabled in editor, disable here
        enabled = false;
    }

    void OnEnable()
    {
        coro = StartCoroutine(TugCoroutine());
    }

    void OnDisable()
    {
        if (coro != null)
        {
            StopCoroutine(coro);
            coro = null;
        }
    }

    IEnumerator TugCoroutine()
    {
        while (true)
        {
            Tug(TugStrength);
            yield return new WaitForSeconds(TugInterval);
        }
    }
}