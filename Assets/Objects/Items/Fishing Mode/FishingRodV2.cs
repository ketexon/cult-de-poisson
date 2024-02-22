using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class FishingRodV2 : MonoBehaviour
{
    [SerializeField] protected InputActionReference tugAction;
    [SerializeField] FishingHookV2 hook;

    public FishingHookV2 Hook => hook;

    virtual protected void Awake()
    {}

    virtual protected void OnDestroy()
    {}

    virtual public void Cast()
    {}
}
