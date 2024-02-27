using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Spear : Item
{
    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] InputActionReference activate;
    [SerializeField] float throwForce;
    [Tooltip("Angle measured in degrees from spear's z-axis (straight forward)")]
    [SerializeField] float throwAngle;
    [SerializeField] float rotateAnimationDuration; 

    bool usingSpear = false;
    bool isRotating = false;
    bool isThrown = false;

    Rigidbody rb;

    void Reset()
    {
        parameters = FindUtil.Asset<GlobalParametersSO>();
    }

    public override void Initialize(InitializeParams initParams)
    {
        base.Initialize(initParams);
        rb = gameObject.GetComponent<Rigidbody>();
    }

    public override void OnUse()
    {
        base.OnUse();

        if (!isRotating && !isThrown) {
            if (!usingSpear) {
                // Get ready to throw the spear
                StartCoroutine(Rotate(90f - throwAngle));
                activate.action.performed += OnThrow;
            } else {
                // Back to resting/idle position
                StartCoroutine(Rotate(-(90f - throwAngle)));
                activate.action.performed -= OnThrow;
            }
        }
    }

    // Rotate spear downwards by [angle] degrees
    IEnumerator Rotate(float angle) {
        isRotating = true;
        float timeElapsed = 0;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = transform.rotation * Quaternion.Euler(angle, 0, 0);
        while (timeElapsed < rotateAnimationDuration) {
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, timeElapsed / rotateAnimationDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation;
        isRotating = false;
        usingSpear = !usingSpear;
    }

    // Throw spear
    void OnThrow(InputAction.CallbackContext ctx) {
        if (!isRotating) {
            isThrown = true;
            activate.action.performed -= OnThrow;
            // Unparent spear so that it doesn't move with the player anymore
            gameObject.transform.parent = null;
            // Make the spear physical
            rb.isKinematic = false;
            rb.useGravity = true;
            // Add force!
            rb.AddForce(0, Mathf.Sin(throwAngle * Mathf.PI/180f) * throwForce, Mathf.Cos(throwAngle * Mathf.PI/180f) * throwForce, ForceMode.Impulse);
        }
    }

    void Update() {
        // Make the spear point in the same way it moves
        transform.up = Vector3.Slerp(transform.up, GetComponent<Rigidbody>().velocity.normalized, Time.deltaTime * 15);
    }

    public override void OnStopUsingItem()
    {
        activate.action.performed -= OnThrow;
        base.OnStopUsingItem();
    }
}
