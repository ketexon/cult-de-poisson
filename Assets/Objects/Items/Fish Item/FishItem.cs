using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using Unity.VisualScripting;

public class FishItem : Item
{
    [SerializeField] InputActionReference altUseItemAction;
    [SerializeField] InputActionReference placeItemAction;
    [SerializeField] PlayerInventorySO inventory;
    [SerializeField] float throwVelocity = 10.0f;
    [SerializeField] float throwAngularVelocity = 10.0f;
    [SerializeField] float placeMaxDistance = 10.0f;
    FishSO fishSO;
    GameObject fishGO;
    private Rigidbody thrownFishRigidbody;

    public void SetFish(FishSO fishSO)
    {
        this.fishSO = fishSO;
        if (fishGO)
        {
            Destroy(fishGO);
        }
        fishGO = Instantiate(fishSO.InHandPrefab, this.transform);
    }

    public void Awake()
    {
        altUseItemAction.action.performed += OnAltUse;
        placeItemAction.action.performed += OnPlaceUse;
    }

    public void OnDestroy()
    {
        altUseItemAction.action.performed -= OnAltUse;
        placeItemAction.action.performed -= OnPlaceUse;
    }

    public void OnAltUse(InputAction.CallbackContext ctx)
    {
        if(!inventory.HasFish(fishSO))
        {
            return;
        }

        // Get the camera's front vector and make it a unit vector
        Vector3 front = mainCamera.transform.forward;
        front.Normalize();

        // Get the current position of the in hand
        Transform temp = fishGO.transform;

        // Destroy the in hand fish
        if(fishGO)
        {
            Destroy(fishGO);
        }

        // Remove the fish from the inventory
        inventory.RemoveFish(fishSO);

        // Instantiate thrown fish and set its velocity
        fishGO = Instantiate(fishSO.PhysicalPrefab, temp.position, temp.rotation);
        Fish fish = fishGO.GetComponent<Fish>();
        fish.InitializeOnGround();
        fishGO.tag = "GroundFish";
        thrownFishRigidbody = fishGO.GetComponent<Rigidbody>(); 
        thrownFishRigidbody.velocity = throwVelocity * front;

        // Give the thrown fish angular velocity
        thrownFishRigidbody.angularVelocity = throwAngularVelocity * front;
        playerItem.CycleItem(1);
    }
    
    public void OnPlaceUse(InputAction.CallbackContext ctx)
    {
        // Get the front and position vectors of the camera
        Transform cameraTransform = mainCamera.transform;
        Vector3 front = cameraTransform.forward;
        Vector3 position = cameraTransform.position;

        RaycastHit hit;

        if(fishGO == null)
        {
            return;
        }

        // If the raycast hits something, place the fish
        if(Physics.Raycast(position, front, out hit, placeMaxDistance))
        {
            // If the fish is placed on the ground, add it to the inventory
            if(hit.collider.tag == "GroundFish")
            {
                FishSO ground = hit.collider.GetComponent<Fish>().FishSO;
                inventory.AddFish(ground);
                DestroyImmediate(hit.collider.gameObject);
                return;
            }

            // If we are trying to place the fish on the ground, place it
            if(inventory.HasFish(fishSO))
            {
                Quaternion rotation = fishGO.transform.rotation;

                // Destroy the in hand fish
                if(fishGO)
                {
                    Destroy(fishGO);
                }

                // Remove the fish from the inventory
                inventory.RemoveFish(fishSO);

                // Instantiate the fish at the hit location
                fishGO = Instantiate(fishSO.PhysicalPrefab, hit.point, rotation);
                fishGO.tag = "GroundFish";
                Fish fish = fishGO.GetComponent<Fish>();
                fish.InitializeOnGround();
                playerItem.CycleItem(1);
                return;
            }
        }
    }
}

