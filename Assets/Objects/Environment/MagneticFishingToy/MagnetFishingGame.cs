using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class MagnetFishingGame : Interactable
{
    [SerializeField] InputActionReference activate;
    [SerializeField] InputActionReference move;
    [SerializeField] float rodTipYOffset;
    [SerializeField] CinemachineVirtualCamera fishingVCam;
    [SerializeField] CinemachineVirtualCamera caughtVCam;

    [SerializeField] float rotateDownDuration = 0.4f;
    [SerializeField] float waitDuration = 0.4f;
    [SerializeField] float rotateUpDuration = 0.4f;
    [SerializeField] float rotationAngle = 0.7f;
    [SerializeField] Canvas cutsceneCanvas;
    [SerializeField] VideoPlayer cutsceneVideo;

    public override bool TargetInteractVisible => true;
    public override bool TargetInteractEnabled => true;
    public override string TargetInteractMessage => "to use fishing toy";

    private Transform turntable;
    private Transform rod;
    private Transform rodTip;
    private Transform hook;
    private float rodInitialY;
    private float rodHorizontalRotateSpeed;

    MagnetFishingGameFish caughtFish = null;

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
        Vector3 turntable_center = turntable.GetComponent<Renderer>().bounds.center;
        turntable.RotateAround(turntable_center, Vector3.up, 1.0f);
        rod.Rotate(0f, rodHorizontalRotateSpeed * 0.2f, 0f, Space.Self);
        // turntable.Rotate(0, 1.0f, 0);
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

            yield return new WaitForSeconds(1);
            cutsceneVideo.Play();
            cutsceneVideo.prepareCompleted += OnVideoStart;
            cutsceneVideo.loopPointReached += OnVideoEnd;
        }

        StartCoroutine(Coro());
    }

    void OnVideoStart(VideoPlayer _)
    {
        cutsceneVideo.prepareCompleted -= OnVideoStart;
        cutsceneCanvas.enabled = true;
    }

    void OnVideoEnd(VideoPlayer _)
    {
        cutsceneVideo.loopPointReached -= OnVideoEnd;

        cutsceneCanvas.enabled = false;

        fishingVCam.enabled = false;
        caughtVCam.enabled = false;

        Player.Instance.PopActionMap();
        Destroy(this);
    }
}