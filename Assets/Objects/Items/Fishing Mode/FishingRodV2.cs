using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class FishingRodV2 : MonoBehaviour
{
    [SerializeField] InputActionReference castAction;

    virtual protected void Awake()
    {
        castAction.action.performed += OnCast;
    }

    virtual protected void OnDestroy()
    {
        castAction.action.performed -= OnCast;
    }

    virtual protected void OnCast(InputAction.CallbackContext obj)
    {}
}
