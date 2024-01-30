using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

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
    GameObject thrownFish;
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
        thrownFish = Instantiate(fishSO.PhysicalPrefab, temp.position, temp.rotation);
        thrownFishRigidbody = thrownFish.GetComponent<Rigidbody>(); 
        thrownFishRigidbody.velocity = throwVelocity * front;

        // Give the thrown fish angular velocity
        thrownFishRigidbody.angularVelocity = throwAngularVelocity * front;
    }
    
    public void OnPlaceUse(InputAction.CallbackContext ctx)
    {
        // Create a raycast from the center of the screen
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // If the raycast hits something, place the fish
        if(Physics.Raycast(ray, out RaycastHit hit, placeMaxDistance))
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

}
