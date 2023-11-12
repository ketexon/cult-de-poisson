using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static bool Contains(this LayerMask mask, int layer)
    {
        return (mask & 1 << layer) > 0;
    }
}
