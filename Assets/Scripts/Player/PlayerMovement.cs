using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput), typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] new Camera camera;
    [SerializeField] float mouseSensitivity;
    [SerializeField] float maxPitch = 85;
    [SerializeField] float speed = 3;
    Rigidbody rb;

    public Vector2 Angle => new Vector2(camera.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y);
    public Vector3 Position => transform.position;

    void Reset()
    {
        camera = GetComponentInChildren<Camera>();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        var delta = ctx.ReadValue<Vector2>();
        var deltaRotY = Quaternion.Euler(delta.x * mouseSensitivity * Vector3.up);
        transform.rotation = transform.rotation * deltaRotY;
        rb.velocity = deltaRotY * rb.velocity;

        var cameraEulerX = (camera.transform.rotation.eulerAngles.x + 180) % 360 - 180;
        var newCameraEulerX = Mathf.Clamp(cameraEulerX - delta.y * mouseSensitivity, -maxPitch, maxPitch);
        var deltaCameraEulerX = newCameraEulerX - cameraEulerX;
        
        camera.transform.rotation = camera.transform.rotation * Quaternion.Euler(deltaCameraEulerX * Vector3.right);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        var dir = ctx.ReadValue<Vector2>();
        rb.velocity = transform.rotation * new Vector3(dir.x, 0, dir.y) * speed;
    }
}
