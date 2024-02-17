using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(LineRenderer))]
public class FishingLine : MonoBehaviour
{
    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] Transform tip;
    [SerializeField] GameObject defaultBobPrefab;

    GameObject bobPrefab = null;
    FishingHook hook = null;

    GameObject bobGO;
    FishingBob bob;

    LineRenderer lineRenderer;

    Vector3[] points = new Vector3[3];

    float? bobDistance = null;
    float? hookDistance = null;

    Vector3 rodTipVelocity;

    bool reelingPhase = false;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        bobPrefab = defaultBobPrefab;
    }

    void Reset()
    {
        parameters = FindUtil.Asset<GlobalParametersSO>();
    }

    void OnDestroy()
    {
        if (bobGO)
        {
            Destroy(bobGO);
        }
    }

    public void OnCast(FishingHook hook, Transform tip, Vector3 rodTipVelocity)
    {
        this.tip = tip;
        points[0] = points[1] = tip.position;

        this.hook = hook;
        hook.WaterHitEvent += OnHookHitWater;

        this.rodTipVelocity = rodTipVelocity;

        lineRenderer.positionCount = 2;
    }

    public void SetBobPrefab(GameObject prefab)
    {
        bobPrefab = prefab;
    }

    public void Reel(float amount)
    {
        if (!reelingPhase) return;
        if (bob && bobDistance.HasValue)
        {
            // 0.01f because physics engine instantaneously
            // teleports object to anchor if distance is 0
            bobDistance = bobDistance.Value - amount * Time.deltaTime * parameters.ReelStrength;
            if (bobDistance < 0)
            {
                Destroy(bobGO);
                lineRenderer.positionCount = 2;

                bobGO = null;
                bob = null;

                hook.AttachToRB(tip.GetComponent<Rigidbody>());
                hookDistance = hook.BobDistance;
            }
            else
            {
                bob.Reel(bobDistance.Value);
            }
        }
        else if(hook && hookDistance.HasValue)
        {
            hookDistance = Mathf.Max(0, hookDistance.Value - amount * Time.deltaTime * parameters.ReelStrength);
            hook.Reel(hookDistance.Value);
        }
    }

    void Update()
    {
        if (hook)
        {
            UpdateBob();
            UpdatePoints();
            DrawPoints();
        }
    }

    void UpdatePoints()
    {
        points[0] = hook
            ? hook.transform.position
            : tip.position;
        points[1] = bobGO
            ? bobGO.transform.position
            : tip.position;
        points[2] = tip.position;
    }

    void OnHookHitWater(Vector3 pos)
    {
        points[1] = pos;

        lineRenderer.positionCount = 3;
    }

    void UpdateBob()
    {
        if (!reelingPhase && bobPrefab && !bobGO && (tip.position - hook.transform.position).magnitude > hook.BobDistance)
        {
            AddBob();
        }
    }

    void AddBob()
    {
        bobGO = Instantiate(bobPrefab, tip.position, Quaternion.identity);
        var bobRB = bobGO.GetComponent<Rigidbody>();
        bobRB.velocity = rodTipVelocity;
        hook.AttachToRB(bobRB);

        bob = bobGO.GetComponent<FishingBob>();
        bob.HitWaterEvent += OnBobHitWater;

        lineRenderer.positionCount = 3;
    }

    void OnBobHitWater()
    {
        bob.HitWaterEvent -= OnBobHitWater;
        bobDistance = (tip.position - bob.transform.position).magnitude;
        bob.AttachToTip(tip.gameObject, bobDistance.Value);

        reelingPhase = true;
    }

    void DrawPoints()
    {
        lineRenderer.SetPositions(points);
    }
}
