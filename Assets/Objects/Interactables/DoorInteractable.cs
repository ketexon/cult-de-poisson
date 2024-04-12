using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

// Door, can be interacted with if holding any key fish
// Interact not implemented yet
public class DoorInteractable : Interactable
{
    public override string InteractMessage => InteractEnabled ? "Unlock Door?" : "Door is Locked";

    public void Start() {
        InteractEnabled = false;
        Player.Instance.Item.ItemChangeEvent += updateCanInteract;
    }

    public void updateCanInteract(Item item) {
        StartCoroutine(updateCanInteractAfterDelay(item, 0.02f));
    }
    public override void OnInteract() {
        Debug.Log("interacted");
    }

    public IEnumerator updateCanInteractAfterDelay(Item item, float delay) {
        // Delay because it takes a little while for the fishSO to update
        yield return new WaitForSeconds(delay);

        if (item is FishItem fishItem && fishItem.fishSO.Name == "Key Fish") {
            InteractEnabled = true;
        } else {
            InteractEnabled = false;
        }
    }
}
