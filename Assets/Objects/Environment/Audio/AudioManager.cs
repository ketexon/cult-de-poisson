using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class AudioManager : SingletonBehaviour<AudioManager>
{
    [SerializeField] EventReference pickupEvent;
    [SerializeField] EventReference windEvent;

    EventInstance pickupEventInstance;
    EventInstance windEventInstance;

    void Start()
    {
        pickupEventInstance = RuntimeManager.CreateInstance(pickupEvent);
        windEventInstance = RuntimeManager.CreateInstance(windEvent);
    }

    void OnDestroy()
    {
        pickupEventInstance.release();
        windEventInstance.release();
    }

    public void PlayPickupSound()
    {
        pickupEventInstance.start();
    }

    public void PlayWindSound()
    {
        windEventInstance.start();
    }

    public void StopWindSound()
    {
        windEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}

