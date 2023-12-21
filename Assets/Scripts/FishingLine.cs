using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(LineRenderer))]
public class FishingLine : MonoBehaviour
{
    [SerializeField] Transform tip;
    [SerializeField] GameObject defaultBobPrefab;

    GameObject bobPrefab = null;
    FishingHook hook = null;

    GameObject bobInstance;

    LineRenderer lineRenderer;

    Vector3[] points = new Vector3[3];

    bool hookHitWater = false;
    bool fishCaught = false;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        points[0] = points[1] = tip.position;
        bobPrefab = defaultBobPrefab;
    }

    public void SetHook(FishingHook hook)
    {
        this.hook = hook;
        hook.WaterHitEvent += OnHookHitWater;
        hook.FishCatchEvent += OnFishCatch;

        lineRenderer.positionCount = 2;
    }

    void OnFishCatch(Fish fish)
    {
        fishCaught = true;
    }

    public void SetBobPrefab(GameObject prefab)
    {
        bobPrefab = prefab;
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
        if (hook)
        {
            points[0] = hook.transform.position;
        }
        if (hookHitWater)
        {
            points[2] = tip.position;
            if (fishCaught)
            {
                points[1] = hook.WaterHitPos.Value;
            }
        }
        else
        {
            points[1] = tip.position;
        }
    }

    void OnHookHitWater(Vector3 pos)
    {
        hookHitWater = true;
        points[1] = pos;

        lineRenderer.positionCount = 3;
    }

    void UpdateBob()
    {
        Vector3? bobPos = CalculateBobPosition();
        if (bobPrefab && !bobInstance && bobPos.HasValue)
        {
            bobInstance = Instantiate(bobPrefab, bobPos.Value, Quaternion.identity);
        }
        else if (bobInstance)
        {
            if (bobPos.HasValue)
            {
                bobInstance.transform.position = bobPos.Value;
            }
            else
            {
                bobInstance.transform.position = tip.position;
            }
        }
    }

    Vector3? CalculateBobPosition()
    {
        float distanceLeft = hook.BobDistance;
        for (int i = 0; i < lineRenderer.positionCount - 1; ++i)
        {
            float lineLength = (points[i + 1] - points[i]).magnitude;
            if(lineLength > distanceLeft)
            {
                return points[i] + (points[i + 1] - points[i]) / lineLength * distanceLeft;
            }
            distanceLeft -= lineLength;
        }
        return null;
    }

    void DrawPoints()
    {
        lineRenderer.SetPositions(points);
    }
}
