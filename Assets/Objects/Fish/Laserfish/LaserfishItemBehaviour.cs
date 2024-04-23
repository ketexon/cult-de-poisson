using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserfishItemBehaviour : FishItemBehaviour
{
    //[SerializeField] Vector3 targetPosition = new(0, 0, 1);
    //[SerializeField] float transitionSpeed = 10;

    public override bool TargetInteractVisible => true;
    public override string TargetInteractMessage => turnedOn ? "Turn off Pufferfish :(" : "Turn on Pufferfish uwo";

    bool turnedOn = false;


    //Vector3 initialPosition;

    
    void Start()
    {
        //initialPosition = transform.localPosition;
    }

    public override void OnInteract()
    {
        //turnedOn = !turnedOn;

        //InteractivityChangeEvent?.Invoke(this);
    }

    void Update()
    {
        /*var actualTarget = 
            turnedOn
                ? targetPosition - transform.parent.localPosition
                : initialPosition;
        
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            actualTarget,
            Time.deltaTime * transitionSpeed
        );*/

        
    }
}
