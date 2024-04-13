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
    [SerializeField] ItemSO fishItemSO;
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


    /// <summary>
    /// Initializes the bucket's fish and registers input bindings.
    /// The bucket's fish are spawned programmatically through the FishSOs in the
    /// PlayerInventorySO.
    /// The fish are spawned in one by one with the same transforms as the children in fishSpawnContainer.
    /// The scale of the spawn transform is treated as a bounding box, and the fish are then
    /// moved down in the spawn bounding box as far as possible to the bottom of this bounding box.
    /// </summary>
    /// <param name="initParams"></param>
    public override void Initialize(InitializeParams initParams)
    {
        base.Initialize(initParams);

    }

    void OnEnable()
    {
        // assign fish to each slot in the bucket
        for (int i = 0; i < fishSpawnContainer.childCount; i++)
        {
            // if we have no more slots or we have no more fish, break
            if (i >= inventory.Fish.Count) break;

            Transform spawn = fishSpawnContainer.GetChild(i);
            FishSO fishSO = inventory.Fish[i];

            // spawn the Fish's bucket prefab
            var go = Instantiate(fishSO.InBucketPrefab, spawn.position, spawn.rotation, transform);
            var fish = go.GetComponent<Fish>();
            fish.InitializeBucket();

            // get the physical bounding box of the fish (from BoxCollider)
            // and use itz z as the length
            float length = fish.GetComponent<BoxCollider>().size.z * fish.transform.localScale.z;

            // move the fish as far down as it can in the bucket
            // without touching the bottom
            float deltaHeight = spawn.localScale.z - length;
            go.transform.position += spawn.transform.forward * deltaHeight / 2;

            var bucketFish = fish.gameObject.AddComponent<BucketFish>();
            spawnedFish.Add(bucketFish);
        }
        
        exitAction.action.performed += OnExitBucket;

        cycleFishAction.action.performed += OnCycleFish;
        selectFishAction.action.performed += OnSelectFish;
    }

    void OnDisable()
    {
        StopUsingBucket();

        exitAction.action.performed -= OnExitBucket;

        cycleFishAction.action.performed -= OnCycleFish;
        selectFishAction.action.performed -= OnSelectFish;

        // Destroy all fish
        foreach (BucketFish fish in spawnedFish)
        {
            Destroy(fish.gameObject);
        }
        spawnedFish.Clear();
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

    void OnCycleFish(InputAction.CallbackContext ctx)
    {
        var floatValue = ctx.ReadValue<float>();
        // either -1 or 1 corresponding to input
        var intValue = System.Math.Sign(floatValue);
        if (intValue == 0)
        {
            return;
        }

        int? newSelectedFish;

        // if we are currently selecting the fish,
        // add the intValue and unselect the fish if 
        // we go out of bounds
        if (selectedFish.HasValue)
        {
            newSelectedFish = selectedFish.Value + intValue;
            newSelectedFish =
                newSelectedFish >= 0 && newSelectedFish < spawnedFish.Count
                    ? newSelectedFish
                    : null;
        }
        // if we are not currently selecting a fish,
        // go to the first fish if we cycle forward or the last
        // if we cycle backwards
        else
        {
            newSelectedFish = intValue < 0 ? spawnedFish.Count - 1 : 0;
        }

        SelectFish(newSelectedFish);
    }

    // Called when we press e on a fish
    // switch to fishItem and set the fish to the one selected
    void OnSelectFish(InputAction.CallbackContext ctx)
    {
        if (selectedFish.HasValue)
        {
            var fishSO = spawnedFish[selectedFish.Value].GetComponent<Fish>().FishSO;

            var newItem = playerItem.GetItem(fishItemSO, true) as FishItem;
            newItem.SetFish(fishSO);
            playerItem.EnableItem(newItem, temporary: true);
        }
    }

    void OnExitBucket(InputAction.CallbackContext ctx)
    {
        StopUsingBucket();
    }

    void StopUsingBucket()
    {
        if(playerInput.currentActionMap != null && playerInput.currentActionMap.name != "Gameplay")
        {
            playerInput.SwitchCurrentActionMap("Gameplay");
        }

        virtualCamera.enabled = false;
        InputUI.Instance.SetCrosshairVisible(true);

        if (usingBucket)
        {
            LockCursor.PopLockState();
        }

        usingBucket = false;

        SelectFish(null);
    }

    // when a fish is highlighted, sets the position and rotation
    // on the BucketFish script, which takes care of the lerping
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
