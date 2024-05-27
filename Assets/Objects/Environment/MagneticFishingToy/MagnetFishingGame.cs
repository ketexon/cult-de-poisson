using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MagnetFishingGame : MonoBehaviour
{
    [SerializeField] InputActionReference activate;
    [SerializeField] InputActionReference move;
    [SerializeField] float rodTipYOffset;
    private Transform turntable;
    private Transform rod;
    private Transform rodTip;
    private float rodInitialY;
    private float rodHorizontalRotateSpeed;
    void Start()
    {
        activate.action.performed += OnClick;
        move.action.performed += (InputAction.CallbackContext ctx) => {rodHorizontalRotateSpeed = ctx.ReadValue<Vector2>().x;};
        move.action.canceled += (InputAction.CallbackContext ctx) => {rodHorizontalRotateSpeed = 0;};

        turntable = transform.Find("Turntable");
        rod = transform.Find("MagnetRodPivot");
        rodTip = rod.Find("MagnetRodTip");
        rodInitialY = rodTip.position.y;
        rodHorizontalRotateSpeed = 0;
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
        Debug.Log("Fiiiiiish");
        while (rodInitialY - rodTip.position.y < rodTipYOffset) {
            rod.Rotate(0.4f, 0f, 0f, Space.Self);
            yield return null;
        }
        // Wait
        float timeElapsed = 0;
        while (timeElapsed < 0.2f) {
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        // Rotate Up
        while (rodInitialY - rodTip.position.y > 0) {
            rod.Rotate(-0.7f, 0f, 0f, Space.Self);
            yield return null;
        }
        activate.action.performed += OnClick;
    }
}
