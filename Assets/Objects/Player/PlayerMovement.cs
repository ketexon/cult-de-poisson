using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private string eventName = "event:/Explosion";


    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] new Cinemachine.CinemachineVirtualCamera camera;
    [SerializeField] float mouseSensitivity;
    [SerializeField] float maxPitch = 85;
    [SerializeField] float speed = 3;

    public Cinemachine.CinemachineVirtualCamera Camera => camera;

    CharacterController characterController;

    public Vector2 Angle => new Vector2(camera.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y);
    public Vector3 Position => transform.position;

    Vector3 inputDir = Vector3.zero;

    float lastTimeOnGround;

    void Reset()
    {
        parameters = FindUtil.Asset<GlobalParametersSO>();
        camera = GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
    }

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (characterController.isGrounded)
        {
            lastTimeOnGround = Time.time;
        }

        characterController.Move(CalculateVelocity() * Time.deltaTime);
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        var delta = ctx.ReadValue<Vector2>();
        var deltaRotY = Quaternion.Euler(delta.x * mouseSensitivity * Vector3.up);
        transform.rotation = transform.rotation * deltaRotY;

        inputDir = deltaRotY * inputDir;

        var cameraEulerX = (camera.transform.rotation.eulerAngles.x + 180) % 360 - 180;
        var newCameraEulerX = Mathf.Clamp(cameraEulerX - delta.y * mouseSensitivity, -maxPitch, maxPitch);
        var deltaCameraEulerX = newCameraEulerX - cameraEulerX;
        
        camera.transform.rotation = camera.transform.rotation * Quaternion.Euler(deltaCameraEulerX * Vector3.right);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        var dir = ctx.ReadValue<Vector2>();
        inputDir = transform.rotation * new Vector3(dir.x, 0, dir.y);
        RuntimeManager.PlayOneShot(eventName, transform.position);
    }

    /// <summary>
    /// Used to calculate velocity relative to any plane we are on.
    /// This is to prevent the player from moving into a plane when going uphill
    /// or into air when going downhill. This function also applies gravity.
    /// </summary>
    /// <returns></returns>
    Vector3 CalculateVelocity()
    {
        var velocity = inputDir * speed;
        if (Physics.Raycast(
                transform.position,
                Vector3.down,
                out var hitInfo,
                characterController.height / 2 + 0.2f,
                parameters.GroundLayerMask,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            var newVelocity = Quaternion.FromToRotation(Vector3.up, hitInfo.normal) * velocity;
            if(newVelocity.y < 0)
            {
                // if we are going downhill, make the direction of the velocity parallel
                // to the slope
                // this prevents us from going forward then falling
                velocity = newVelocity;
            }
        }
        velocity += Physics.gravity * (Time.time - lastTimeOnGround);
        return velocity;
    }
}
