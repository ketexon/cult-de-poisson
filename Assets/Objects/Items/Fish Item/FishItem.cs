using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class FishItem : Item
{
    [SerializeField] InputActionReference throwAction;
    [SerializeField] InputActionReference placeItemAction;
    [SerializeField] PlayerInventorySO inventory;
    [SerializeField] float throwVelocity = 10.0f;
    [SerializeField] float throwAngularVelocity = 10.0f;
    [SerializeField] float placeMaxDistance = 10.0f;
    [System.NonSerialized] public FishSO fishSO;
    GameObject fishGO;
    Fish fish;
    private Rigidbody thrownFishRigidbody;

    public override bool CanSwitchItems => inventory.Fish.Count <= inventory.MaxFish;
    public override string SwitchItemsDisabledReason => "inventory full";

    public override IInteractObject InteractItem => fish ? fish.ItemBehaviour : null;

    System.Action inputUIDestructor;

    public void SetFish(FishSO fishSO)
    {
        this.fishSO = fishSO;
        if (fishGO)
        {
            Destroy(fishGO);
        }
        fishGO = Instantiate(fishSO.PhysicalPrefab, this.transform);
        gameObject.SetActive(true); // we must activate before we initialize bucket so that awake is called on Fish
        fish = fishGO.GetComponent<Fish>();
        fish.InitializeBucket();

    }

    public void SetFish(Fish fish)
    {
        this.fishSO = fish.FishSO;
        this.fish = fish;
        if (fishGO)
        {
            Destroy(fishGO);
        }
        fishGO = fish.gameObject;
        fish.transform.SetParent(transform);
        fish.transform.position = this.transform.position;
        fish.transform.localRotation = Quaternion.identity;
        fish.InitializeBucket();
    }

    public void OnEnable()
    {
        throwAction.action.performed += OnThrow;
        placeItemAction.action.performed += OnPlaceUse;

        Player.Instance.Input.controlsChangedEvent.AddListener(OnControlsChanged);
        inputUIDestructor = InputUI.Instance.AddInputUI(throwAction, "to yeet");
    }

    public void OnDisable()
    {
        throwAction.action.performed -= OnThrow;
        placeItemAction.action.performed -= OnPlaceUse;

        Player.Instance.Input.controlsChangedEvent.RemoveListener(OnControlsChanged);
        inputUIDestructor?.Invoke();
    }

    public void OnThrow(InputAction.CallbackContext ctx)
    {
        // Get the camera's front vector and make it a unit vector
        Vector3 front = mainCamera.transform.forward;

        // Remove the fish from the inventory
        inventory.RemoveFish(fishSO);

        fishGO.transform.SetParent(null);
        
        fishGO.GetComponent<Fish>().InitializePhysical();
        // Instantiate thrown fish and set its velocity
        //thrownFish = Instantiate(fishSO.PhysicalPrefab, temp.position, temp.rotation);
        thrownFishRigidbody = fishGO.GetComponent<Rigidbody>(); 
        thrownFishRigidbody.velocity = throwVelocity * front;

        // Give the thrown fish angular velocity
        thrownFishRigidbody.angularVelocity = throwAngularVelocity * front;

        fishGO = null;

        playerItem.CycleItem();    
    }
    
    public void OnPlaceUse(InputAction.CallbackContext ctx)
    {
        // Get the front and position vectors of the camera
        Transform cameraTransform = mainCamera.transform;
        Vector3 front = cameraTransform.forward;
        Vector3 position = cameraTransform.position;

        RaycastHit hit;

        // If the raycast hits something, place the fish
        if(Physics.Raycast(position, front, out hit, placeMaxDistance))
        {
            // Get the current position of the in hand
            Quaternion rotation = fishGO.transform.rotation;

            // Destroy the in hand fish
            if(fishGO)
            {
                Destroy(fishGO);
            }

            // Remove the fish from the inventory
            inventory.RemoveFish(fishSO);

            // Instantiate the fish at the hit location
            Instantiate(fishSO.PhysicalPrefab, hit.point, rotation);
        }
    }

    void OnControlsChanged(PlayerInput _)
    {
        inputUIDestructor?.Invoke();
        inputUIDestructor = InputUI.Instance.AddInputUI(throwAction, "to yeet");
    }
}
