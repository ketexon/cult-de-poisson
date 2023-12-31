using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInteract : MonoBehaviour
{
    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] InputActionReference interactAction;

    Interactable _interactable;
    Interactable Interactable {
        get => _interactable;
        set
        {
            if(value != _interactable)
            {
                if (_interactable)
                {
                    _interactable.CanInteractChangeEvent -= OnInteractableCanInteractChange;
                }
                _interactable = value;
                UpdateUI();
                if (_interactable)
                {
                    _interactable.CanInteractChangeEvent += OnInteractableCanInteractChange;
                }
            }
        }
    }

    PlayerMovement playerMovement;

    System.Action interactableUIDestructor = null;

    void Reset()
    {
        parameters = FindUtil.Asset<GlobalParametersSO>();
    }

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    void OnDestroy()
    {
        if (_interactable)
        {
            _interactable.CanInteractChangeEvent -= OnInteractableCanInteractChange;
        }
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if(!ctx.performed)
        {
            return;
        }
        if(Interactable && Interactable.CanInteract)
        {
            Interactable.OnInteract();
        }
    }

    void Update()
    {
        if(Physics.Raycast(
            transform.position, playerMovement.Camera.transform.forward, 
            out RaycastHit hit, 
            parameters.InteractDistance, 
            parameters.InteractLayerMask
        ))
        {
            Interactable = hit.collider.GetComponent<Interactable>();
        }
        else
        {
            Interactable = null;
        }
    }

    void OnInteractableCanInteractChange(bool _)
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        interactableUIDestructor?.Invoke();
        interactableUIDestructor = null;
        if (Interactable)
        {
            interactableUIDestructor = InputUI.Instance.AddInputUI(
                interactAction, 
                Interactable.InteractMessage, 
                !Interactable.CanInteract
            );
        }
    }
}
