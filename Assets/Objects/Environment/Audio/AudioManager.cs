using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class AudioManager : SingletonBehaviour<AudioManager>
{
    [SerializeField] EventReference pickupEvent;
    [SerializeField] EventReference windEvent;

    EventInstance pickupEventInstance;
    EventInstance windEventInstance;

    Bus inGameBus;

    void Start()
    {
        pickupEventInstance = RuntimeManager.CreateInstance(pickupEvent);
        windEventInstance = RuntimeManager.CreateInstance(windEvent);

        FMODUnity.RuntimeManager.StudioSystem.getBus("bus:/InGame", out inGameBus);
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

    public void PauseInGameAudio()
    {
        inGameBus.setPaused(true);
    }

    public void ResumeInGameAudio()
    {
        inGameBus.setPaused(false);
    }
}

