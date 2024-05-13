using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class LadderInteractable : Interactable
{
    [SerializeField] Transform top;
    [SerializeField] Transform bottom;

    public override string TargetInteractMessage => "Climb ladder";

    public override void OnInteract()
    {
        StartCoroutine(ClimbLadder(Player.Instance.transform.position.y < top.position.y));
    }

    IEnumerator ClimbLadder(bool startingFromBottom)
    {
        PlayerInput playerInput = Player.Instance.Input;
        NavMeshAgent agent = Player.Instance.GetComponent<NavMeshAgent>();
        playerInput.DeactivateInput();
        agent.SetDestination(startingFromBottom ? top.position : bottom.position);
        yield return new WaitWhile(() => agent.pathPending || agent.remainingDistance > 0.1f);
        playerInput.ActivateInput();
        agent.ResetPath();
    }
}
