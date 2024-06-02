using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixPowerLineInteractable : Interactable
{
    [SerializeField] QuestSO quest;
    [SerializeField] new ParticleSystem particleSystem;

    public override string TargetInteractMessage => "to fix power line";

    public override void OnInteract()
    {
        base.OnInteract();

        QuestStateSO.Instance.CompleteQuest(quest);
        particleSystem.Stop();
    }
}
