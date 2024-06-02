using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{
    enum LinkType { Ladder };

    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] new Cinemachine.CinemachineVirtualCamera camera;
    [SerializeField] Transform lookRoot;
    [SerializeField] float mouseSensitivity;
    [SerializeField] float maxPitch = 85;
    [SerializeField] float speed = 3;
    [SerializeField] float ladderClimbSpeed = 0.25f;
    [SerializeField] bool lockCamera = false;

    NavMeshAgent agent;
    Rigidbody rb;
    PlayerInput playerInput;
    new Collider collider;

    public Cinemachine.CinemachineVirtualCamera Camera => camera;

    public Rigidbody Rigidbody => rb;
    public float Pitch => camera.transform.rotation.eulerAngles.x;
    public float Yaw => lookRoot.rotation.eulerAngles.y;
    public Vector2 Angle => new Vector2(camera.transform.rotation.eulerAngles.x, lookRoot.rotation.eulerAngles.y);
    public Vector3 Position => transform.position;

    Vector3 inputDir = Vector3.zero;

    float lastTimeOnGround;
    bool movingOnLink;

    /// <summary>
    /// Pitch and yaw range. pitchRange is guarenteed to be between 0,360. 
    /// yawRange is gaurenteed to be between 0, 720
    /// </summary>
    Vector2? pitchRange = null, yawRange = null;

    bool shouldReenableAgent = false;

    Vector2 lookSpeed = Vector2.zero;

    public void SetPhysicsEnabled(bool enabled)
    {
        if (enabled)
        {
            SetPhysicsEnabledImpl(true);
        }
        else
        {
            shouldReenableAgent = true;
        }
    }

    void Reset()
    {
        parameters = FindUtil.Asset<GlobalParametersSO>();
        camera = GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
        if (!movingOnLink && agent.isOnOffMeshLink)
        {
            movingOnLink = true;
            HandleLinkMovement(LinkType.Ladder);
        }
        else if (agent.isOnNavMesh && !movingOnLink && playerInput.inputIsActive)
        {
            var displacement = CalculateVelocity() * Time.deltaTime;
            agent.Move(displacement);
        }

        UpdateLook(lookSpeed * Time.deltaTime);
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || lockCamera) return;

        var delta = ctx.ReadValue<Vector2>();

        UpdateLook(delta);
    }

    public void OnLookSpeed(InputAction.CallbackContext ctx)
    {
        //if (ctx.started) return;
        lookSpeed = ctx.ReadValue<Vector2>();
        Debug.Log(lookSpeed);
    }

    void UpdateLook(Vector2 delta)
    {
        if (delta == Vector2.zero) return;

        var playerEulerY = lookRoot.rotation.eulerAngles.y;
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

        lookRoot.rotation = lookRoot.rotation * deltaRotY;

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
        inputDir = lookRoot.rotation * new Vector3(dir.x, 0, dir.y);
    }

    public void DisableKeyboardMovement()
    {
        GetComponent<PlayerInput>().DeactivateInput();
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
                agent.height / 2 + 0.2f,
                parameters.GroundLayerMask,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            var newVelocity = Quaternion.FromToRotation(Vector3.up, hitInfo.normal) * velocity;
            if (newVelocity.y < 0)
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

    void OnCollisionEnter(Collision collision)
    {
        if (shouldReenableAgent)
        {
            SetPhysicsEnabledImpl(false);
        }
    }

    void SetPhysicsEnabledImpl(bool enabled)
    {
        if (enabled)
        {
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            collider.enabled = true;

            agent.enabled = false;
        }
        else
        {
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.None;

            collider.enabled = false;

            agent.enabled = true;
        }

        shouldReenableAgent = false;

    }

    void HandleLinkMovement(LinkType linkType)
    {
        switch (linkType)
        {
            case LinkType.Ladder:
                StartCoroutine(HandleLadderLinkMovement());
                break;
            default:
                break;
        }
    }

    IEnumerator HandleLadderLinkMovement()
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        bool isGoingUp = data.startPos.y < data.endPos.y;

        yield return TraverseLadder(data, isGoingUp);

        movingOnLink = false;
    }

    IEnumerator TraverseLadder(OffMeshLinkData data, bool startFromBottom)
    {
        Vector3 bottom = data.offMeshLink.startTransform.position;
        Vector3 aboveBottom = new(bottom.x, transform.position.y, bottom.z);

        while (Vector3.Distance(transform.position, aboveBottom) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, aboveBottom, Time.deltaTime * ladderClimbSpeed);
            yield return null;
        }

        float verticalDifference = transform.position.y - data.endPos.y - 0.75f;

        while (Mathf.Abs(verticalDifference) > 0.1f && (startFromBottom ? verticalDifference < 0 : verticalDifference > 0))
        {
            transform.position += (startFromBottom ? 1 : -1) * ladderClimbSpeed * Time.deltaTime * Vector3.up;
            verticalDifference = transform.position.y - data.endPos.y - 0.75f;
            yield return null;
        }

        Vector3 aboveTop = new(data.endPos.x, transform.position.y, data.endPos.z);
        while (Vector3.Distance(transform.position, aboveTop) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, aboveTop, Time.deltaTime * ladderClimbSpeed);
            yield return null;
        }
        agent.CompleteOffMeshLink();
    }
}
