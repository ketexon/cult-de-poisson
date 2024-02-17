using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator), typeof(LineRenderer))]
public class DeepSeaRod : FishingRodV2
{
    [SerializeField] GameObject tip;
    [SerializeField] GameObject hook;

    Animator animator;
    LineRenderer lineRenderer;

    ConfigurableJoint hookJoint;

    Vector3[] linePositions = new Vector3[2];

    override protected void Awake()
    {
        base.Awake();

        animator = GetComponent<Animator>();
        lineRenderer = GetComponent<LineRenderer>();

        hookJoint = hook.GetComponent<ConfigurableJoint>();
    }

    protected override void OnCast(InputAction.CallbackContext obj)
    {
        base.OnCast(obj);
        animator.SetTrigger("Cast");
    }

    void Update()
    {
        linePositions[0] = tip.transform.position;
        linePositions[1] = hook.transform.position;

        lineRenderer.SetPositions(linePositions);
    }

    void DropLine()
    {
        hookJoint.xMotion = ConfigurableJointMotion.Free;
        hookJoint.yMotion = ConfigurableJointMotion.Free;
        hookJoint.zMotion = ConfigurableJointMotion.Free;
    }
}
