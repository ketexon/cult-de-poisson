using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInteract : MonoBehaviour
{
    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] InputActionReference interactAction;

    class InteractTarget
    {
        public System.Action Callback;
        public string Message;
        public bool Disabled;
        public System.Action InputUIDestructor;
    }

    List<InteractTarget> interactTargets = new();

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
        if (Interactable && Interactable.CanInteract)
        {
            Interactable.OnInteract();
        }
        else
        {
            foreach (var interactTarget in interactTargets)
            {
                if (!interactTarget.Disabled)
                {
                    interactTarget.Callback();
                    return;
                }
            }
        }
    }

    public System.Action AddInteract(System.Action callback, string message, bool disabled = false)
    {
        InteractTarget target = new()
        {
            Callback = callback,
            Message = message,
            Disabled = disabled,
        };
        interactTargets.Add(target);
        UpdateUI();
        return () =>
        {
            interactTargets.Remove(target);
            target.InputUIDestructor?.Invoke();
        };
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
        bool hasInteractTarget = false;
        if (Interactable)
        {
            interactableUIDestructor = InputUI.Instance.AddInputUI(
                interactAction,
                Interactable.InteractMessage,
                !Interactable.CanInteract
            );
            hasInteractTarget = Interactable.CanInteract;
            InputUI.Instance.SetCrosshairEnabled(Interactable.CanInteract);
        }
        else
        {
            InputUI.Instance.SetCrosshairEnabled(false);
        }
        foreach (var interactTarget in interactTargets)
        {
            interactTarget.InputUIDestructor?.Invoke();
            interactTarget.InputUIDestructor = InputUI.Instance.AddInputUI(
                interactAction,
                interactTarget.Message,
                interactTarget.Disabled || hasInteractTarget
            );
            hasInteractTarget = true;
        }
    }
}
