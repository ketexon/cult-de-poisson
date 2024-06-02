using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elder : Interactable
{
    [SerializeField] float rotSpeed = 2;
    [SerializeField] QuestSO quest;

    public override string TargetInteractMessage => "to interact";

    Animator animator;
    Quaternion targetRot;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        QuestStateSO.Instance.CompletedEvent += OnQuestCompleted;
    }

    void OnDestroy()
    {
        QuestStateSO.Instance.CompletedEvent -= OnQuestCompleted;
    }

    void OnQuestCompleted(QuestSO q)
    {
        if(q == quest)
        {
            animator.SetTrigger("quest_complete");
            Destroy(this);
        }
    }

    public override void OnInteract()
    {
        base.OnInteract();

        animator.SetTrigger("interact");
    }

    void Update()
    {
        targetRot = Quaternion.LookRotation((Player.Instance.transform.position - transform.position).ProjectXZ(), Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * rotSpeed);
    }
}
