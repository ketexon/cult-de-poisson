using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using static UnityEditor.PlayerSettings;
using System.Net.Sockets;
using System.Security.Cryptography;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Bucket))]
[CanEditMultipleObjects]
public class BucketEditor : Editor
{
    void OnSceneGUI()
    {
        var bucket = target as Bucket;
        var so = new SerializedObject(bucket);
        so.Update();

        var pos = bucket.transform.position;

        var originalCenter = (bucket.SpawnMax + bucket.SpawnMin) / 2;
        var newCenter = Handles.PositionHandle(originalCenter + pos, Quaternion.Euler(0, 0, 0)) - pos;
        var deltaCenter = newCenter - originalCenter;

        var min = Handles.PositionHandle(pos + bucket.SpawnMin, Quaternion.Euler(180, 180, 180)) - pos;
        var max = Handles.PositionHandle(pos + bucket.SpawnMax, Quaternion.identity) - pos;

        var normalizedMin = Extensions.Min(min, max) + deltaCenter;
        var normalizedMax = Extensions.Max(min, max) + deltaCenter;

        var spawnMinSP = so.FindProperty("SpawnMin");
        var spawnMaxSP = so.FindProperty("SpawnMax");

        if((spawnMinSP.vector3Value - normalizedMin).magnitude > 0.00001f)
        {
            spawnMinSP.vector3Value = normalizedMin;
        }
        if ((spawnMinSP.vector3Value - normalizedMax).magnitude > 0.00001f)
        {
            spawnMaxSP.vector3Value = normalizedMax;
        }

        so.ApplyModifiedProperties();
    }
}

#endif

public class Bucket : Item
{
    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    [SerializeField] PlayerInventorySO inventory;
    [SerializeField] ItemSO fishItem;
    [SerializeField] float maxRaycastDistance = 5.0f;

    [FormerlySerializedAs("SpawnMin")]
    [SerializeField] public Vector3 SpawnMin;

    [FormerlySerializedAs("SpawnMax")]
    [SerializeField] public Vector3 SpawnMax;

    [SerializeField] InputActionReference pointAction;
    [SerializeField] InputActionReference clickAction;
    [SerializeField] InputActionReference exitAction;

    Vector2 pointPos = Vector2.zero;

    List<GameObject> spawnedFish = new();

    Fish hoveredFish = null;

    bool usingBucket = false;

    void Reset()
    {
        parameters = FindUtil.Asset<GlobalParametersSO>();
        virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        inventory = FindUtil.Asset<PlayerInventorySO>();
    }
    void OnDrawGizmos()
    {
        Gizmos.color = new Color()
        {
            r = 0,
            g = 0,
            b = 1,
            a = 0.2f,
        };
        Gizmos.DrawCube(
            transform.position + (SpawnMax + SpawnMin) / 2, 
            (SpawnMax - SpawnMin).Abs()
        );
    }
    public override void Initialize(InitializeParams initParams)
    {
        base.Initialize(initParams);

        foreach(var fish in inventory.Fish)
        {
            var point = GetSpawnPoint();
            var go = Instantiate(fish.InBucketPrefab, point, Quaternion.identity, transform);
            spawnedFish.Add(go);
        }

        pointAction.action.performed += OnPoint;
        clickAction.action.performed += OnClick;
        exitAction.action.performed += OnExitBucket;
    }

    Vector3 GetSpawnPoint()
    {
        return (SpawnMax - SpawnMin) * Random.Range(0f, 1f) + SpawnMin + transform.position;
    }

    public override void OnStopUsingItem()
    {
        OnExitBucket(new());

        pointAction.action.performed -= OnPoint;
        clickAction.action.performed -= OnClick;
        exitAction.action.performed -= OnExitBucket;

        foreach(var fish in spawnedFish)
        {
            Destroy(fish);
        }
        foreach(Transform t in transform)
        {
            if(t.gameObject != virtualCamera.gameObject)
            {
                Destroy(t.gameObject);
            }
        }

        virtualCamera.enabled = false;

        IEnumerator DestroyCoroutine()
        {
            yield return new WaitForEndOfFrame();
            while (cinemachineBrain.IsBlending)
            {
                yield return new WaitForEndOfFrame();
            }
            base.OnStopUsingItem();
        }
        StartCoroutine(DestroyCoroutine());
    }

    public override void OnUse()
    {
        base.OnUse();

        usingBucket = true;

        playerInput.SwitchCurrentActionMap("Bucket");
        virtualCamera.enabled = true;
        InputUI.Instance.SetCrosshairVisible(false);

        LockCursor.PushLockState(CursorLockMode.None);
    }

    void OnPoint(InputAction.CallbackContext ctx)
    {
        pointPos = ctx.ReadValue<Vector2>();


        Ray ray = mainCamera.ScreenPointToRay(pointPos);
        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            float.MaxValue,
            parameters.BucketFishLayerMask
        ))
        {
            var fish = hit.collider.GetComponent<Fish>();
            hoveredFish = fish;
        }
    }

    void OnClick(InputAction.CallbackContext ctx)
    {
        if (hoveredFish)
        {
            var fishSO = hoveredFish.FishSO;

            playerItem.EnableTemporaryItem(fishItem);

            (playerItem.EnabledItem as FishItem).SetFish(fishSO);
        }
    }

    void OnExitBucket(InputAction.CallbackContext ctx)
    {
        playerInput.SwitchCurrentActionMap("Gameplay");
        virtualCamera.enabled = false;
        InputUI.Instance.SetCrosshairVisible(true);

        if (usingBucket)
        {
            LockCursor.PopLockState();
        }

        usingBucket = false;
    }
}
