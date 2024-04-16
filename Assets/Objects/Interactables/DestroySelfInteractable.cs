using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DestroySelfInteractable : Interactable
{
    [SerializeField] string interactmessage;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnInteract()
    {
        base.OnInteract();
        Destroy(gameObject);
    }

    public override string TargetInteractMessage
    {
        get
        {
            return interactmessage;
        }
    }
}
