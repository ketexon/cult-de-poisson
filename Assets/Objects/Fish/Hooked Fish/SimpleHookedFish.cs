using System.Collections;
using UnityEngine;

public class SimpleHookedFish : HookedFish
{
    [SerializeField] float minTugInterval = 2;
    [SerializeField] float maxTugInterval = 3;

    [SerializeField] float minTugStrength = 1;
    [SerializeField] float maxTugStrength = 4;

    [SerializeField] float tugAcceleration = 2.0f;

    float TugInterval => Random.Range(minTugInterval, maxTugInterval);
    float TugStrength => Random.Range(minTugStrength, maxTugStrength);

    Coroutine coro = null;

    void FixedUpdate()
    {
        if (enabled && RodTipTransform && inWater)
        {
            var delta = (rb.position - PlayerTransform.position).ProjectXZ().normalized;
            var force = delta * tugAcceleration;
            rb.AddForce(force, ForceMode.Acceleration);
            Debug.Log($"VEL: {force}");

            transform.rotation = Quaternion.LookRotation(delta, Vector3.up);
        }
    }

    override protected void OnEnable()
    {
        base.OnEnable();
        coro = StartCoroutine(TugCoroutine());

        rb.constraints = RigidbodyConstraints.FreezeRotationZ;
    }

    override protected void OnDisable()
    {
        if (coro != null)
        {
            StopCoroutine(coro);
            coro = null;
        }

        rb.constraints = RigidbodyConstraints.None;
    }

    IEnumerator TugCoroutine()
    {
        while (true)
        {
            Tug(TugStrength);
            yield return new WaitForSeconds(TugInterval);
        }
    }
}