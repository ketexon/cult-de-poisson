using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderInteractable : Interactable
{
    public override string TargetInteractMessage => "Climb ladder";

    public override void OnInteract()
    {
        Player.Instance.Input.DeactivateInput();
    }
}
