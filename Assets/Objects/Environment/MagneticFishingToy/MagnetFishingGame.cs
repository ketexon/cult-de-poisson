using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Audio;
using UnityEngine.Experimental.Video;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class MagnetFishingGame : Interactable
{
    const int LATENCY_MS = 50;
    const int DRIFT_MS = 1;
    const float DRIFT_CORRECTION_PERCENTAGE = 0.5f;

    [SerializeField] InputActionReference activate;
    [SerializeField] InputActionReference move;
    [SerializeField] float rodTipYOffset;
    [SerializeField] CinemachineVirtualCamera fishingVCam;
    [SerializeField] CinemachineVirtualCamera caughtVCam;

    [SerializeField] float rotateDownDuration = 0.4f;
    [SerializeField] float waitDuration = 0.4f;
    [SerializeField] float rotateUpDuration = 0.4f;
    [SerializeField] float rotationAngle = 0.7f;
    [SerializeField] float caughtCameraHoldDuration = 1.5f;
    [SerializeField] Canvas cutsceneCanvas;
    [SerializeField] CanvasGroup cutsceneGroup;
    [SerializeField] VideoPlayer cutsceneVideo;
    [SerializeField] CanvasGroup cutsceneFadeGroup;
    [SerializeField] float whiteTransitionTime = 0.5f;

    public override bool TargetInteractVisible => !endedGame;
    public override bool TargetInteractEnabled => true;
    public override string TargetInteractMessage => "to use fishing toy";

    private Transform turntable;
    private Transform rod;
    private Transform rodTip;
    private Transform hook;
    private float rodInitialY;
    private float rodHorizontalRotateSpeed;

    MagnetFishingGameFish caughtFish = null;

    bool endedGame = false;

    AudioSampleProvider audioSampleProvider;
    
    FMOD.Sound cutsceneSound;
    FMOD.Channel mChannel;
    FMOD.CREATESOUNDEXINFO mExinfo;
    
    private List<float> mBuffer = new List<float>();

    private int mSampleRate;
    private uint mDriftThreshold;
    private uint mTargetLatency;
    private uint mAdjustedLatency;
    private int mActualLatency;

    private uint mTotalSamplesWritten;
    private uint mMinimumSamplesWritten = uint.MaxValue;

    private uint mLastReadPosition;
    private uint mTotalSamplesRead;


    void Start()
    {
        turntable = transform.Find("Turntable");
        rod = transform.Find("MagnetRodPivot");
        rodTip = rod.Find("MagnetRodTip");
        hook = transform.Find("MagnetHook");

        rodInitialY = rodTip.position.y;
        rodHorizontalRotateSpeed = 0;

        rod.gameObject.SetActive(false);
        hook.gameObject.SetActive(false);
        fishingVCam.enabled = false;
    }

    public override void OnInteract()
    {
        base.OnInteract();

        rod.gameObject.SetActive(true);
        hook.gameObject.SetActive(true);
        fishingVCam.enabled = true;
        Player.Instance.PushActionMap("FishingToy");

        activate.action.performed += OnClick;
        move.action.performed += (InputAction.CallbackContext ctx) => { rodHorizontalRotateSpeed = ctx.ReadValue<Vector2>().x; };
        move.action.canceled += (InputAction.CallbackContext ctx) => { rodHorizontalRotateSpeed = 0; };
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (endedGame) return;

        Vector3 turntable_center = turntable.GetComponent<Renderer>().bounds.center;
        turntable.RotateAround(turntable_center, Vector3.up, 1.0f);
        rod.Rotate(0f, rodHorizontalRotateSpeed * 0.2f, 0f, Space.Self);
        // turntable.Rotate(0, 1.0f, 0);
    }

    void Update()
    {
        UpdateAudio();
    }

    void OnClick(InputAction.CallbackContext ctx) {
        activate.action.performed -= OnClick;
        StartCoroutine(Fish());
    }

    // Get Fiiiiish
    IEnumerator Fish() {
        // Rotate Down
        float startTime = Time.time;
        float t = 0;

        Quaternion startRot = rod.localRotation;
        Quaternion targetRot = rod.localRotation 
            * Quaternion.Euler(rotationAngle, 0, 0);

        for (t = 0; t < 1; t = (Time.time - startTime) / rotateDownDuration) {
            rod.localRotation = Quaternion.Lerp(startRot, targetRot, t);
            yield return null;
        }
        rod.localRotation = targetRot;

        // Wait
        yield return new WaitForSeconds(waitDuration);

        startTime = Time.time;
        // Rotate Up
        for (t = 0; t < 1; t = (Time.time - startTime) / rotateUpDuration)
        {
            rod.localRotation = Quaternion.Lerp(targetRot, startRot, t);
            yield return null;
        }
        rod.localRotation = startRot;

        if (caughtFish)
        {
            EndFishing();
        }
        else
        {
            activate.action.performed += OnClick;
        }
    }

    public void OnCatchFish(MagnetFishingGameFish f)
    {
        caughtFish = f;
    }

    void EndFishing()
    {
        IEnumerator Coro()
        {
            fishingVCam.enabled = false;

            caughtVCam.LookAt = caughtFish.transform;
            caughtVCam.Follow = caughtFish.transform;
            caughtVCam.enabled = true;

            yield return null;
            while (Player.Instance.CinemachineBrain.IsBlending)
            {
                yield return null;
            }

            yield return new WaitForSeconds(caughtCameraHoldDuration);

            cutsceneCanvas.enabled = true;

            AudioManager.Instance.FadeOutInGameVolume();

            float startTime = Time.time;
            for(float t = 0; t < 1; t = (Time.time - startTime)/whiteTransitionTime)
            {
                cutsceneFadeGroup.alpha = t;
                yield return null;
            }
            cutsceneFadeGroup.alpha = 1;

            cutsceneVideo.audioOutputMode = VideoAudioOutputMode.APIOnly;

            mSampleRate = (int)(cutsceneVideo.GetAudioSampleRate(0) * cutsceneVideo.playbackSpeed);

            mDriftThreshold = (uint)(mSampleRate * DRIFT_MS) / 1000;
            mTargetLatency = (uint)(mSampleRate * LATENCY_MS) / 1000;
            mAdjustedLatency = mTargetLatency;
            mActualLatency = (int)mTargetLatency;

            var numChannels = cutsceneVideo.GetAudioChannelCount(0);

            mExinfo = new()
            {
                cbsize = Marshal.SizeOf<FMOD.CREATESOUNDEXINFO>(),
                numchannels = numChannels,
                defaultfrequency = mSampleRate,
                length = mTargetLatency * (uint)numChannels * sizeof(float),
                format = FMOD.SOUND_FORMAT.PCMFLOAT,
            };

            FMODUnity.RuntimeManager.CoreSystem.createSound(
                "",
                FMOD.MODE.LOOP_NORMAL | FMOD.MODE.OPENUSER,
                ref mExinfo,
                out cutsceneSound
            );

            cutsceneVideo.Play();
            cutsceneVideo.prepareCompleted += OnVideoPrepared;
            cutsceneVideo.loopPointReached += OnVideoEnd;
        }

        StartCoroutine(Coro());
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        audioSampleProvider = cutsceneVideo.GetAudioSampleProvider(0);
        audioSampleProvider.sampleFramesAvailable += AudioSampleFramesAvailable;
        audioSampleProvider.enableSampleFramesAvailableEvents = true;
        audioSampleProvider.freeSampleFrameCountLowThreshold = audioSampleProvider.maxSampleFrameCount - mTargetLatency;

        cutsceneVideo.prepareCompleted -= OnVideoPrepared;
        cutsceneGroup.alpha = 1;
        Player.Instance.Camera.enabled = false;

        IEnumerator Coro()
        {
            float startTime = Time.time;
            for (float t = 0; t < 1; t = (Time.time - startTime) / whiteTransitionTime)
            {
                cutsceneFadeGroup.alpha = 1 - t;
                yield return null;
            }
            cutsceneFadeGroup.alpha = 0;
        }

        StartCoroutine(Coro());
    }

    void OnVideoEnd(VideoPlayer _)
    {
        cutsceneSound.release();

        IEnumerator Coro()
        {
            float startTime = Time.time;
            for (float t = 0; t < 1; t = (Time.time - startTime) / whiteTransitionTime)
            {
                cutsceneFadeGroup.alpha = t;
                yield return null;
            }

            Player.Instance.Camera.enabled = true;

            cutsceneFadeGroup.alpha = 1;
            cutsceneGroup.alpha = 0;
            
            fishingVCam.enabled = false;
            caughtVCam.enabled = false;

            AudioManager.Instance.FadeInInGameVolume();

            startTime = Time.time;
            for (float t = 0; t < 1; t = (Time.time - startTime) / whiteTransitionTime)
            {
                cutsceneFadeGroup.alpha = 1 - t;
                yield return null;
            }
            cutsceneFadeGroup.alpha = 0;

            cutsceneCanvas.enabled = false;

            Player.Instance.PopActionMap();
            Destroy(this);
        }

        endedGame = true;
        InteractivityChangeEvent?.Invoke(this);
        cutsceneVideo.loopPointReached -= OnVideoEnd;

        hook.gameObject.SetActive(false);
        rod.gameObject.SetActive(false);

        StartCoroutine(Coro());
    }

    void AudioSampleFramesAvailable(AudioSampleProvider provider, uint sampleFrameCount)
    {
        using (NativeArray<float> buffer = new NativeArray<float>((int)sampleFrameCount * provider.channelCount, Allocator.Temp))
        {
            uint written = provider.ConsumeSampleFrames(buffer);
            mBuffer.AddRange(buffer);

            /*
             * Drift compensation
             * If we are behind our latency target, play a little faster
             * If we are ahead of our latency target, play a little slower
             */
            uint samplesWritten = (uint)buffer.Length;
            mTotalSamplesWritten += samplesWritten;

            if (samplesWritten != 0 && (samplesWritten < mMinimumSamplesWritten))
            {
                mMinimumSamplesWritten = samplesWritten;
                mAdjustedLatency = Math.Max(samplesWritten, mTargetLatency);
            }

            int latency = (int)mTotalSamplesWritten - (int)mTotalSamplesRead;
            mActualLatency = (int)((0.93f * mActualLatency) + (0.03f * latency));

            int playbackRate = mSampleRate;
            if (mActualLatency < (int)(mAdjustedLatency - mDriftThreshold))
            {
                playbackRate = mSampleRate - (int)(mSampleRate * (DRIFT_CORRECTION_PERCENTAGE / 100.0f));
            }
            else if (mActualLatency > (int)(mAdjustedLatency + mDriftThreshold))
            {
                playbackRate = mSampleRate + (int)(mSampleRate * (DRIFT_CORRECTION_PERCENTAGE / 100.0f));
            }
            mChannel.setFrequency(playbackRate);
        }
    }

    void UpdateAudio()
    {
        if (!mChannel.hasHandle() && mTotalSamplesWritten > mAdjustedLatency)
        {
            FMOD.ChannelGroup mMasterChannelGroup;
            FMODUnity.RuntimeManager.CoreSystem.getMasterChannelGroup(out mMasterChannelGroup);
            FMODUnity.RuntimeManager.CoreSystem.playSound(cutsceneSound, mMasterChannelGroup, false, out mChannel);
        }

        if (mBuffer.Count > 0 && mChannel.hasHandle() && cutsceneSound.hasHandle())
        {
            uint readPosition;
            mChannel.getPosition(out readPosition, FMOD.TIMEUNIT.PCMBYTES);

            /*
             * Account for wrapping
             */
            uint bytesRead = readPosition - mLastReadPosition;
            if (readPosition < mLastReadPosition)
            {
                bytesRead += mExinfo.length;
            }

            if (bytesRead > 0 && mBuffer.Count >= bytesRead)
            {
                /*
                 * Fill previously read data with fresh samples
                 */
                IntPtr ptr1, ptr2;
                uint len1, len2;
                var res = cutsceneSound.@lock(mLastReadPosition, bytesRead, out ptr1, out ptr2, out len1, out len2);
                if (res != FMOD.RESULT.OK) Debug.LogError(res);

                // Though exinfo.format is float, data retrieved from Sound::lock is in bytes,
                // therefore we only copy (len1+len2)/sizeof(float) full float values across
                int sampleLen1 = (int)(len1 / sizeof(float));
                int sampleLen2 = (int)(len2 / sizeof(float));
                int samplesRead = sampleLen1 + sampleLen2;
                float[] tmpBuffer = new float[samplesRead];

                mBuffer.CopyTo(0, tmpBuffer, 0, tmpBuffer.Length);
                mBuffer.RemoveRange(0, tmpBuffer.Length);

                if (len1 > 0)
                {
                    Marshal.Copy(tmpBuffer, 0, ptr1, sampleLen1);
                }
                if (len2 > 0)
                {
                    Marshal.Copy(tmpBuffer, sampleLen1, ptr2, sampleLen2);
                }

                res = cutsceneSound.unlock(ptr1, ptr2, len1, len2);
                if (res != FMOD.RESULT.OK) Debug.LogError(res);
                mLastReadPosition = readPosition;
                mTotalSamplesRead += (uint)samplesRead;
            }
        }
    }
}
