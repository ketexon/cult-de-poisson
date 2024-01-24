using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishZone : MonoBehaviour
{
    [SerializeField] GlobalParametersSO globalParams;
    [SerializeField] FishZoneSO properties;

    FishingHook hook = null;
    Coroutine catchFishCoroutine = null;

    void Reset()
    {
        globalParams = FindUtil.Asset<GlobalParametersSO>();
    }


    /// <summary>
    /// Returns true if the hook will succeed in catching a fish.
    /// This can be used to, for example, check if the hook is baited
    /// and only catch the fish if there is bait.
    /// </summary>
    bool ValidateHook(FishingHook hook)
    {
        return true;
    }

    public void OnTriggerEnter(Collider other)
    {
        // if the hook enters this fish zone
        if(globalParams.HookLayerMask.Contains(other.gameObject.layer))
        {
            FishingHook hook = other.GetComponent<FishingHook>();
            if (ValidateHook(hook))
            {
                this.hook = hook;
                // wait some time before catching a fish
                catchFishCoroutine = StartCoroutine(FishCatchDelay());
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        // if the hook exits the fishing zone
        var otherLayerMask = 1 << other.gameObject.layer;
        if (globalParams.HookLayerMask.Contains(other.gameObject.layer))
        {
            FishingHook hook = other.GetComponent<FishingHook>();

            // remove the hook and stop the catch fish coroutine
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
        if (hook)
        {
            var fish = properties.Fish[Random.Range(0, properties.Fish.Count)];
            hook.OnCatchFish(fish);
        }
    }
}
