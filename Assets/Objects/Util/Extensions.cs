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


    public static Vector2 XY(this Vector3 v) => new(v.x, v.y);
    public static Vector2 XZ(this Vector3 v) => new(v.x, v.z);
    public static Vector2 YZ(this Vector3 v) => new(v.y, v.z);


    public static Vector3 WithZ(this Vector2 v, float z) => new(v.x, v.y, z);

    public static Vector3 WithX(this Vector3 v, float x) => new(x, v.y, v.z);
    public static Vector3 WithY(this Vector3 v, float y) => new(v.x, y, v.z);
    public static Vector3 WithZ(this Vector3 v, float z) => new(v.x, v.y, z);

    public static Vector3 WithXY(this Vector3 v, Vector2 xy) => new(xy.x, xy.y, v.z);
    public static Vector3 WithXZ(this Vector3 v, Vector2 xz) => new(xz.x, v.y, xz.y);
    public static Vector3 WithYZ(this Vector3 v, Vector2 yz) => new(v.x, yz.x, yz.y);

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

    public static float Volume(this Vector3 length) => length.x * length.y * length.z;

    /// <summary>
    /// Returns a random vector between two vectors.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static Vector3 Random(Vector3 min, Vector3 max) => new Vector3(
        (max - min).x * UnityEngine.Random.value,
        (max - min).y * UnityEngine.Random.value,
        (max - min).z * UnityEngine.Random.value
    ) + min;
}
