using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishInteractable : Interactable
{
    [SerializeField] string InteractMessageTemplate = "Pick up {0}";
    private string fishName;

    public void Awake()
    {
        Fish temp = GetComponent<Fish>();
        fishName = temp.FishSO.Name;

    }
    public override string TargetInteractMessage => string.Format(InteractMessageTemplate, fishName);
}
