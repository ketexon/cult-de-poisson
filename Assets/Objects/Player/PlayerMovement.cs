using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] new Cinemachine.CinemachineVirtualCamera camera;
    [SerializeField] float mouseSensitivity;
    [SerializeField] float maxPitch = 85;
    [SerializeField] float speed = 3;

    public Cinemachine.CinemachineVirtualCamera Camera => camera;

    NavMeshAgent agent;

    public float Pitch => camera.transform.rotation.eulerAngles.x;
    public float Yaw => transform.rotation.eulerAngles.y;
    public Vector2 Angle => new Vector2(camera.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y);
    public Vector3 Position => transform.position;

    Vector3 inputDir = Vector3.zero;

    float lastTimeOnGround;

    /// <summary>
    /// Pitch and yaw range. pitchRange is guarenteed to be between 0,360. 
    /// yawRange is gaurenteed to be between 0, 720
    /// </summary>
    Vector2? pitchRange = null, yawRange = null;

    void Reset()
    {
        parameters = FindUtil.Asset<GlobalParametersSO>();
        camera = GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        var translation = speed * Time.deltaTime * inputDir;
        agent.Move(translation);
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        var delta = ctx.ReadValue<Vector2>();

        var playerEulerY = transform.rotation.eulerAngles.y;
        float newPlayerEulerY = playerEulerY + delta.x * mouseSensitivity;

        if (yawRange is Vector2 yr)
        {
            newPlayerEulerY = Extensions.ClampAngle(
                newPlayerEulerY,
                yr.x,
                yr.y
            );
        }

        var deltaPlayerEulerY = newPlayerEulerY - playerEulerY;
        var deltaRotY = Quaternion.Euler(deltaPlayerEulerY * Vector3.up);

        transform.rotation = transform.rotation * deltaRotY;
        
        inputDir = deltaRotY * inputDir;

        var cameraEulerX = (camera.transform.rotation.eulerAngles.x + 180) % 360 - 180;
        var newCameraEulerX = Mathf.Clamp(cameraEulerX - delta.y * mouseSensitivity, -maxPitch, maxPitch);

        if (pitchRange is Vector2 pr)
        {
            newCameraEulerX = Extensions.ClampAngle(
                newCameraEulerX,
                pr.x,
                pr.y
            );
        }

        var deltaCameraEulerX = newCameraEulerX - cameraEulerX;
        
        camera.transform.rotation = camera.transform.rotation * Quaternion.Euler(deltaCameraEulerX * Vector3.right);
    }

    /// <summary>
    /// Confines the camera between the pitch and yaw min and maxes.
    /// If any parameter is null, it is unconfined.
    /// Pitch is the x rotation (corresponds to moving mouse up and down), 
    /// yaw is the y rotation (corresponds to moving mouse left and right)
    /// </summary>
    /// <param name="yawMin"></param>
    /// <param name="yawMax"></param>
    /// <param name="pitchMin"></param>
    /// <param name="pitchMax"></param>
    public void Confine(Vector2? yawRange = null, Vector2? pitchRange = null)
    {
        this.yawRange = Normalize(yawRange);
        this.pitchRange = Normalize(pitchRange);

        // normalizes min value to [0,360)
        // then updates max value to match it
        Vector2? Normalize(Vector2? v)
        {
            if (v is null) return null;
            float x = Extensions.NormalizeAngle360(v.Value.x);
            float y = Extensions.NormalizeAngle360(v.Value.y);
            return x < y ? new(x, y) : new(x, y + 360);
        }
    }

    public void ConfineRelative(Vector2? yawRange = null, Vector2? pitchRange = null)
    {
        Confine(
            yawRange is Vector2 yr ? Vector2.one * Yaw + yr : null,
            yawRange is Vector2 pr ? Vector2.one * Pitch + pr : null
        );
    }

    public void Unconfine()
    {
        Confine();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        var dir = ctx.ReadValue<Vector2>();
        inputDir = transform.rotation * new Vector3(dir.x, 0, dir.y);
    }
}
