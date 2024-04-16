using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EelFishItemBehaviour : FishItemBehaviour
{
    private int AttachedInteractables = 0;
    private Interactable AttachedInteractable = null;
    public override bool AgentInteractVisible(Interactable interactable) => interactable is EelInteractable;
    public override string AgentInteractMessage(Interactable _) => "Attach eel to this interactable";
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
            }
            else if(AttachedInteractables == 1)
            {
                if(AttachedInteractable == eelInteractable)
                {
                    Debug.Log("EelFishItemBehaviour: Detached Eel");
                    AttachedInteractable = null;
                    AttachedInteractables = 0;
                    return;
                }
                else
                {
                    Debug.Log("EelFishItemBehaviour: Attached eel to second interactable");
                    AttachEel(AttachedInteractable, eelInteractable);
                    AttachedInteractable = null;
                    AttachedInteractables = 0;
                    return;
                }
            }
            else if(AttachedInteractables >= 2)
            {
                Debug.Log("EelFishItemBehaviour: Too many interactables attached");
                return;
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
        fish.InitializeBucket();
        fish.transform.rotation = fish.transform.rotation * Rotation;
        fish.transform.localScale = new Vector3(1, 1, Stretch);
    }
}