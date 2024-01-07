using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Bucket : Item
{
    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    [SerializeField] PlayerInventorySO inventory;
    [SerializeField] ItemSO fishItem;
    [SerializeField] float maxRaycastDistance = 5.0f;

    [SerializeField] InputActionReference pointAction;
    [SerializeField] InputActionReference clickAction;
    [SerializeField] InputActionReference exitAction;

    [SerializeField] Transform fishSpawnContainer;

    Vector2 pointPos = Vector2.zero;

    List<GameObject> spawnedFish = new();

    Fish hoveredFish = null;

    bool usingBucket = false;

    void Reset()
    {
        parameters = FindUtil.Asset<GlobalParametersSO>();
        virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        inventory = FindUtil.Asset<PlayerInventorySO>();
        fishSpawnContainer = FindUtil.Query<Transform>(this)
            .InChildren
            .NameContains("spawn", insensitive: true)
            .Execute();
    }

    public override void Initialize(InitializeParams initParams)
    {
        base.Initialize(initParams);

        for(int i = 0; i < fishSpawnContainer.childCount; i++)
        {
            if (i >= inventory.Fish.Count) break;

            Transform spawn = fishSpawnContainer.GetChild(i);
            FishSO fishSO = inventory.Fish[i];

            var go = Instantiate(fishSO.InBucketPrefab, spawn.position, spawn.rotation, transform);
            var fish = go.GetComponent<Fish>();
            fish.InitializeBucket();

            float length;

            length = fish.GetComponent<BoxCollider>().size.z * fish.transform.localScale.z;

            float deltaHeight = spawn.localScale.z - length;
            go.transform.position += spawn.transform.forward * deltaHeight / 2;
            spawnedFish.Add(go);
        }

        pointAction.action.performed += OnPoint;
        clickAction.action.performed += OnClick;
        exitAction.action.performed += OnExitBucket;
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
