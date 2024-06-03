using FMOD.Studio;
using FMODUnity;
using System.Collections;
using UnityEngine;

public class AudioManager : SingletonBehaviour<AudioManager>
{
    [SerializeField] EventReference pickupEvent;
    [SerializeField] EventReference windEvent;
    [SerializeField, ParamRef] string inGameVolumeParam;
    [SerializeField] float fadeDuration;

    EventInstance pickupEventInstance;
    EventInstance windEventInstance;

    Bus inGameBus;

    float lastInGameVolume;

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

    public void FadeOutInGameVolume()
    {
        RuntimeManager.StudioSystem.getParameterByName(inGameVolumeParam, out lastInGameVolume);
        IEnumerator Coro()
        {
            float startTime = Time.time;
            for(float t = 0; t < 1; t = (Time.time - startTime)/fadeDuration)
            {
                float v = lastInGameVolume * (1 - t);
                RuntimeManager.StudioSystem.setParameterByName(inGameVolumeParam, v);
                yield return null;
            }
        }
        StartCoroutine(Coro());
    }

    public void FadeInInGameVolume()
    {
        IEnumerator Coro()
        {
            float startTime = Time.time;
            for (float t = 0; t < 1; t = (Time.time - startTime) / fadeDuration)
            {
                float v = lastInGameVolume * t;
                RuntimeManager.StudioSystem.setParameterByName(inGameVolumeParam, v);
                yield return null;
            }
        }
        StartCoroutine(Coro());
    }
}

