using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishZone : MonoBehaviour
{
    [SerializeField] GlobalParametersSO globalParams;
    [SerializeField] FishZoneSO properties;


    FishingHook hook = null;
    Coroutine catchFishCoroutine = null;
    bool hookCaughtFish = false;

    void Reset()
    {
        globalParams = FindUtil.Asset<GlobalParametersSO>();
    }


    /// <summary>
    /// Returns true if the hook will succeed in catching a fish
    /// </summary>
    bool ValidateHook(FishingHook hook)
    {
        return true;
    }

    public void OnTriggerEnter(Collider other)
    {
        if(globalParams.HookLayerMask.Contains(other.gameObject.layer))
        {
            FishingHook hook = other.GetComponent<FishingHook>();
            if (ValidateHook(hook))
            {
                this.hook = hook;
                catchFishCoroutine = StartCoroutine(FishCatchDelay());
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        var otherLayerMask = 1 << other.gameObject.layer;
        if (globalParams.HookLayerMask.Contains(other.gameObject.layer))
        {
            FishingHook hook = other.GetComponent<FishingHook>();
            if (this.hook == hook)
            {
                this.hook = null;
            }
            if (catchFishCoroutine != null)
            {
                StopCoroutine(catchFishCoroutine);
                catchFishCoroutine = null;
            }
        }
    }

    public IEnumerator FishCatchDelay()
    {
        float seconds = globalParams.GetRandomFishZoneDelay();
        yield return new WaitForSeconds(seconds);
        hookCaughtFish = true;
        if (hook)
        {
            var fish = properties.Fish[Random.Range(0, properties.Fish.Count)];
            hook.OnCatchFish(fish);
        }
    }
}
