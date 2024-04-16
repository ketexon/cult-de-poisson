using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FishZone : MonoBehaviour
{
    [SerializeField] GlobalParametersSO globalParams;
    [SerializeField] FishZoneSO properties;


    FishingHook hook = null;
    Coroutine catchFishCoroutine = null;

    /// <summary>
    /// All the box colliders on this fish zone.
    /// All of them represent area that the fish can be in.
    /// </summary>
    List<BoxCollider> boxColliders;

    float totalBoxColliderVolume;

    List<Fish> spawnedFish = new();

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

    void Awake()
    {
        totalBoxColliderVolume = 0;
        boxColliders = new(GetComponents<BoxCollider>());
        foreach(var boxCollider in boxColliders)
        {
            var length = boxCollider.size;
            var volume = boxCollider.size.Volume();

            totalBoxColliderVolume += volume;

            var nFish = properties.FishDensity * volume;

            // instantiate that number of fish
            for (int i = 0; i < nFish; ++i)
            {
                // spawn a random fish at a random point in the box
                var fishSO = properties.Fish[Random.Range(0, properties.Fish.Count)];
                var point = Extensions.Random(boxCollider.bounds.min, boxCollider.bounds.max);

                var fishGO = Instantiate(fishSO.InWaterPrefab, point, Quaternion.identity, transform);
                var fish = fishGO.GetComponent<Fish>();
                spawnedFish.Add(fish);

                fish.InitializeWater(this);
            }
        }
    }

    /// <summary>
    /// Returns a random point in the fishing zone.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetRandomPoint()
    {
        float volumeLeft = totalBoxColliderVolume;
        foreach(var boxCollider in boxColliders)
        {
            var volume = boxCollider.size.Volume();
            if (Random.value > volume / volumeLeft)
            {
                volumeLeft -= volume;
                continue;
            }

            return Extensions.Random(boxCollider.bounds.min, boxCollider.bounds.max);
        }
        return transform.position;
    }

    /// <summary>
    /// Check whether a point is in the fishing zone.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool Contains(Vector3 point)
    {
        var localPoint = point;
        foreach(var boxCollider in boxColliders)
        {
            if (boxCollider.bounds.Contains(localPoint))
            {
                return true;
            }
        }
        return false;
    }
}
