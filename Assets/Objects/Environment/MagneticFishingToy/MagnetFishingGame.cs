using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MagnetFishingGame : MonoBehaviour
{
    [SerializeField] InputActionReference activate;
    [SerializeField] float rodAnimationDuration;
    private Transform turntable;
    private Transform rod;
    void Start()
    {
        activate.action.performed += OnClick;
        turntable = transform.Find("Turntable");
        rod = transform.Find("MagnetRodPivot");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 turntable_center = turntable.GetComponent<Renderer>().bounds.center;
        turntable.RotateAround(turntable_center, Vector3.up, 1.0f);
        // turntable.Rotate(0, 1.0f, 0);
    }

    void OnClick(InputAction.CallbackContext ctx) {
        activate.action.performed -= OnClick;
        StartCoroutine(Fish());
    }

    // Get Fiiiiish
    IEnumerator Fish() {
        // Rotate Down
        float timeElapsed = 0;
        while (timeElapsed < rodAnimationDuration) {
            rod.Rotate(0.7f, 0f, 0f, Space.Self);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        // Wait
        timeElapsed = 0;
        while (timeElapsed < 0.2f) {
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        // Rotate Up
        timeElapsed = 0;
        while (timeElapsed < rodAnimationDuration) {
            rod.Rotate(-0.7f, 0f, 0f, Space.Self);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        activate.action.performed += OnClick;
    }
}
