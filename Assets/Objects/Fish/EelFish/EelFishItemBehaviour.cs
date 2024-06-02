using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EelFishItemBehaviour : FishItemBehaviour
{
    [SerializeField] PlayerInventorySO inventory;

    private int AttachedInteractables = 0;
    private Interactable AttachedInteractable = null;
    public override bool AgentInteractVisible(Interactable interactable) => interactable is EelInteractable;
    public override string AgentInteractMessage(Interactable interactable) => 
        interactable == AttachedInteractable 
            ? "to detact eel"
            : string.Format(
                "to attach eel to {0} endpoint", 
                AttachedInteractable == null ? "first" : "second"
            );
    
    [SerializeField] GameObject EelPhysical;
    [SerializeField] float EelStretchFudgeFactor = 1.0f;
    public override void OnInteract(Interactable target)
    {
        if(target is EelInteractable eelInteractable)
        {
            if(AttachedInteractables == 0)
            {
                Debug.Log("EelFishItemBehaviour: Attached eel to first interactable");
                AttachedInteractable = eelInteractable;
                AttachedInteractables++;

                InteractivityChangeEvent?.Invoke(this);
            }
            else if(AttachedInteractables == 1)
            {
                if(AttachedInteractable == eelInteractable)
                {
                    Debug.Log("EelFishItemBehaviour: Detached Eel");
                    AttachedInteractable = null;
                    AttachedInteractables = 0;

                    InteractivityChangeEvent?.Invoke(this);
                    return;
                }
                else
                {
                    Debug.Log("EelFishItemBehaviour: Attached eel to second interactable");
                    AttachEel(AttachedInteractable, eelInteractable);
                    inventory.RemoveFish(GetComponent<Fish>().FishSO);
                    Player.Instance.Item.CycleItem();
                    
                    return;
                }
            }
        }
    }

    public void AttachEel(Interactable I1, Interactable I2)
    {
        Vector3 Pos1 = I1.transform.position;
        Vector3 Pos2 = I2.transform.position;
        Vector3 MidPoint = (Pos1 + Pos2) / 2;
        float Stretch = Vector3.Distance(Pos1, Pos2) * EelStretchFudgeFactor;
        var go = Instantiate(EelPhysical, MidPoint, Quaternion.identity);
        var fish = go.GetComponent<Fish>();
        Quaternion Rotation = Quaternion.FromToRotation(fish.transform.forward, Pos2 - Pos1);

        fish.InitializeStatic();
        fish.transform.rotation = fish.transform.rotation * Rotation;
        fish.transform.localScale = new Vector3(1, 1, Stretch);

        var q = (I1 as EelInteractable).Quest;
        QuestStateSO.Instance.CompleteQuest(q);
    }
}