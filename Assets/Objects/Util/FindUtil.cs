using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class FindUtil
{
    public static LayerMask Layer(string layer)
    {
        layer = layer.ToLower();
        int exactMatch = LayerMask.NameToLayer(layer);
        if (exactMatch != -1) return exactMatch;
        for(int i = 0; i < 32; ++i)
        {
            string layerName = LayerMask.LayerToName(i).ToLower();
            if (layerName.Contains(layer))
            {
                return new LayerMask() { value = 1 << i };
            }
        }
        return new LayerMask();
    }

    /// <summary>
    /// Finds the first asset of type T that includes `name` in its path, or
    /// the first asset if `name` is `null`. If an asset could not be found, returns `null`.
    /// </summary>
    public static T Asset<T>(string name = null)
        where T : Object
    {
#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        foreach(string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (name == null || assetPath.Contains(name))
            {
                return AssetDatabase.LoadAssetAtPath<T>(assetPath);
            }
        }
        return null;
#else
        return null;
#endif
    }
}
