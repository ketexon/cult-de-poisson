using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static bool Contains(this LayerMask mask, int layer)
    {
        return (mask & 1 << layer) > 0;
    }

    public static Vector3 ProjectXZ(this Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }

    public static Vector3 Abs(this Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    public static Vector3 WithZ(this Vector2 v, float z)
    {
        return new Vector3(v.x, v.y, z);
    }

    public static Vector3 Min(Vector3 v1, Vector3 v2) => new(
        Mathf.Min(v1.x, v2.x),
        Mathf.Min(v1.y, v2.y),
        Mathf.Min(v1.z, v2.z)
    );

    public static Vector3 Max(Vector3 v1, Vector3 v2) => new(
        Mathf.Max(v1.x, v2.x),
        Mathf.Max(v1.y, v2.y),
        Mathf.Max(v1.z, v2.z)
    );
}
