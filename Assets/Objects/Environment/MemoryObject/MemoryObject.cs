using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Changes the active child object whenever the player looks away
/// MAKE SURE TO ADD DESIRED OBJECTS AS CHILDREN
///     Memory Object will rotate through the list of children
/// 
/// There's a mesh on the root object which should never be visible
///     it should always be covered by a child mesh
///     but the bigger the root mesh is, the later the object will change
///     it's for triggering OnBecameInvisible
///     if its not covered, make the root mesh smaller
///     
/// If it looks too obvious, I can remove the root mesh and  make the object changing more precise
/// </summary>
class MemoryObject : MonoBehaviour
{

    // Index of the child that is currently active (rest are inactive)
    [SerializeField] int activeChildIndex = 0;

    void Start()
    {    
        activateObject();
    }

    void OnBecameInvisible() {
        activeChildIndex++;
        activeChildIndex = activeChildIndex % transform.childCount;
        activateObject();
    }


    private void activateObject() {
        for (int i = 0; i < transform.childCount; i++) {
            if (i == activeChildIndex) {
                transform.GetChild(i).gameObject.SetActive(true);
            } else {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
