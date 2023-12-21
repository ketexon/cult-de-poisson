using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(LineRenderer))]
public class FishingLineOld : MonoBehaviour
{
    [SerializeField] Transform tip;
    [SerializeField] float pointDistance = 0.1f;
    [SerializeField] float bobDistance = 3.0f;
    [SerializeField] GameObject defaultBobPrefab;
    [SerializeField] float bounciness;
    int bobPoint => nPoints - Mathf.FloorToInt(bobDistance / pointDistance);
    GameObject bobPrefab = null;
    FishingHook hook = null;

    FishingBob bobInstance = null;
    Rigidbody bobRB = null;
    bool bobInWater = false;

    LineRenderer lineRenderer;

    int nPoints = 0;
    Vector3[] previousPoints = new Vector3[100];
    // note: the last point only APPEARS to be connected to the hook.
    // to prevent stretching the rope, it does not actually follow the hook, and is likely one step behind the hook
    Vector3[] points = new Vector3[100];

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        AddPoint(tip.position, tip.position);
        bobPrefab = defaultBobPrefab;
    }

    public void OnBobHitWater()
    {
        //hook.OnBobHitWater(bobInstance, bobDistance, bounciness);
        bobInWater = true;
    }

    public void SetHook(FishingHook hook)
    {
        this.hook = hook;
    }

    public void SetBobPrefab(GameObject prefab)
    {
        bobPrefab = prefab;
    }

    void Update()
    {
        points[0] = tip.position;
        if (hook)
        {
            var hookPoint = points[nPoints - 1];
            // if the distance between the last point and the tip could fit another vertex, add one
            if ((hookPoint - hook.transform.position).magnitude >= pointDistance)
            {
                AddPoint(hook.transform.position, hook.transform.position);
            }
        }
        UpdatePoints();
        UpdateBob();
        DrawPoints();
    }

    /// METHOD:
    /// https://www.owlree.blog/posts/simulating-a-rope.html
    void UpdatePoints()
    {
        // Verlet integration
        for (int i = 1; i < nPoints - 1; ++i)
        {
            if(bobInstance && bobPoint == i)
            {
                points[i] = bobInstance.transform.position;
            }
            else
            {
                Vector3 pos = points[i];
                Vector3 prevPos = previousPoints[i];
                points[i] = 2 * pos - prevPos + Time.deltaTime * Time.deltaTime * Physics.gravity;
                previousPoints[i] = pos;
            }
        }

        // Jakobson method for constraints
        Vector3 GetDeltaDisp(int i, int j)
        {
            Vector3 disp = points[j] - points[i];
            float length = disp.magnitude;
            float dLength = length - pointDistance;
            Vector3 dir = disp / length;
            return dLength * dir;
        }

        void RelaxConstraintLeftFixed(int i, int j)
        {
            var disp = GetDeltaDisp(i, j);
            points[j] -= disp;
        }

        void RelaxConstraint(int i, int j)
        {
            var disp = GetDeltaDisp(i, j);
            points[i] += disp / 2;
            points[j] -= disp / 2;
        }

        void RelaxConstraintRightFixed(int i, int j)
        {
            var disp = GetDeltaDisp(i, j);
            points[i] += disp;
        }

        // if points.Count is 2, then the constraints are always satisfied + doesn't work with code
        if (nPoints > 2)
        {
            for (int iter = 0; iter < nPoints; ++iter)
            {
                RelaxConstraintLeftFixed(0, 1);
                for (int i = 1; i < nPoints - 2; ++i)
                {
                    if(bobInstance && i == bobPoint)
                    {
                        RelaxConstraintLeftFixed(i, i + 1);
                    }
                    else if(bobInstance && i + 1 == bobPoint)
                    {
                        RelaxConstraintRightFixed(i, i + 1);
                    }
                    else {
                        RelaxConstraint(i, i + 1);
                    }
                }
                RelaxConstraintRightFixed(nPoints - 2, nPoints - 1);
            }
        }
    }

    void UpdateBob()
    {
        if (bobPrefab && !bobInstance && bobPoint >= 0)
        {
            var bobGO = Instantiate(bobPrefab, points[bobPoint], Quaternion.identity);
            bobInstance = bobGO.GetComponent<FishingBob>();
            bobInstance.SetLine(this);
            bobRB = bobInstance.GetComponent<Rigidbody>();
            Debug.Log("SPAWNED BOB");
        }
        if (bobInstance && !bobInWater)
        {
            Vector3 pos = points[bobPoint];
            Vector3 dir = points[bobPoint + 1] - points[bobPoint];
            dir.Normalize();
            pos += dir * ((pos - hook.transform.position).magnitude - bobDistance);
            bobInstance.transform.position = pos;
        }
    }

    void DrawPoints()
    {
        lineRenderer.positionCount = nPoints;
        lineRenderer.SetPositions(points);
    }

    void AddPoint(Vector3 point, Vector3 prevPoint)
    {
        if(nPoints >= points.Length)
        {
            Array.Resize(ref points, points.Length * 2);
            Array.Resize(ref previousPoints, points.Length * 2);
        }
        points[nPoints] = point;
        previousPoints[nPoints] = prevPoint;
        ++nPoints;
    }
}
