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
    [SerializeField] InputActionReference cycleFishAction;
    [SerializeField] InputActionReference selectFishAction;
    [SerializeField] InputActionReference clickAction;
    [SerializeField] InputActionReference exitAction;

    [SerializeField] Transform fishSelectedTransform;
    [SerializeField] Transform fishSpawnContainer;

    Vector2 pointPos = Vector2.zero;

    List<BucketFish> spawnedFish = new();
    int? selectedFish = null;

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

            float length = fish.GetComponent<BoxCollider>().size.z * fish.transform.localScale.z;

            float deltaHeight = spawn.localScale.z - length;
            go.transform.position += spawn.transform.forward * deltaHeight / 2;

            var bucketFish = fish.gameObject.AddComponent<BucketFish>();
            spawnedFish.Add(bucketFish);
        }

        pointAction.action.performed += OnPoint;
        clickAction.action.performed += OnClick;
        exitAction.action.performed += OnExitBucket;

        cycleFishAction.action.performed += OnCycleFish;
        selectFishAction.action.performed += OnSelectFish;
    }

    public override void OnStopUsingItem()
    {
        StopUsingBucket();

        pointAction.action.performed -= OnPoint;
        clickAction.action.performed -= OnClick;
        exitAction.action.performed -= OnExitBucket;

        cycleFishAction.action.performed -= OnCycleFish;
        selectFishAction.action.performed -= OnSelectFish;

        foreach (BucketFish fish in spawnedFish)
        {
            Destroy(fish.gameObject);
        }
        foreach (Transform t in transform)
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

    void OnCycleFish(InputAction.CallbackContext ctx)
    {
        var floatValue = ctx.ReadValue<float>();
        var intValue = System.Math.Sign(floatValue);
        if (intValue == 0)
        {
            return;
        }

        int? newSelectedFish;

        if (selectedFish.HasValue)
        {
            newSelectedFish = selectedFish.Value + intValue;
            newSelectedFish =
                newSelectedFish >= 0 && newSelectedFish < spawnedFish.Count
                    ? newSelectedFish
                    : null;
        }
        else
        {
            newSelectedFish = intValue < 0 ? spawnedFish.Count - 1 : 0;
        }

        SelectFish(newSelectedFish);
    }

    void OnSelectFish(InputAction.CallbackContext ctx)
    {
        if (selectedFish.HasValue)
        {
            var fishSO = spawnedFish[selectedFish.Value].GetComponent<Fish>().FishSO;

            playerItem.EnableTemporaryItem(fishItem);

            (playerItem.EnabledItem as FishItem).SetFish(fishSO);
        }
    }

    void OnExitBucket(InputAction.CallbackContext ctx)
    {
        playerInput.SwitchCurrentActionMap("Gameplay");
        StopUsingBucket();
    }

    // This is separated into two functions because
    // the destructor needs to do everything BUT swapping input maps
    // if you swap input maps to the currently active
    // input map, it will refire events.
    void StopUsingBucket()
    {
        virtualCamera.enabled = false;
        InputUI.Instance.SetCrosshairVisible(true);

        if (usingBucket)
        {
            LockCursor.PopLockState();
        }

        usingBucket = false;

        SelectFish(null);
    }

    void SelectFish(int? index)
    {
        if (selectedFish.HasValue)
        {
            spawnedFish[selectedFish.Value].TargetLocalPos = spawnedFish[selectedFish.Value].StartLocalPos;
            spawnedFish[selectedFish.Value].TargetLocalRotation = spawnedFish[selectedFish.Value].StartLocalRotation;
        }
        selectedFish = index;
        if (selectedFish.HasValue)
        {
            spawnedFish[selectedFish.Value].TargetLocalPos = fishSelectedTransform.localPosition;
            spawnedFish[selectedFish.Value].TargetLocalRotation = fishSelectedTransform.localRotation;
        }
    }
}
