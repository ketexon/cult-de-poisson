using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

#if UNITY_EDITOR
using UnityEngine;
#endif

public static class FindUtil
{
    public class FindQuery<T> where T: UnityEngine.Component
    {
        enum Direction { Self, Parents, Children, All };

        MonoBehaviour monobehavior;
        Direction direction = Direction.Self;

        List<System.Func<T, bool>> filters = new();

        public FindQuery(MonoBehaviour monobehavior)
        {
            this.monobehavior = monobehavior;
        }

        public FindQuery<T> InParents { 
            get
            {
                direction = Direction.Parents;
                return this;
            }
        }

        public FindQuery<T> InSelf
        {
            get
            {
                direction = Direction.Self;
                return this;
            }
        }

        public FindQuery<T> InChildren
        {
            get 
            {
                direction = Direction.Children;
                return this;
            }
        }

        public FindQuery<T> InAll
        {
            get
            {
                direction = Direction.All;
                return this;
            }
        }

        public FindQuery<T> NameEquals(string name, bool insensitive = false)
        {
            if (insensitive)
            {
                name = name.ToLower();
                filters.Add(comp => comp.gameObject.name.ToLower() == name);
            }
            else
            {
                filters.Add(comp => comp.gameObject.name == name);
            }
            return this;
        }

        public FindQuery<T> NameContains(string name, bool insensitive = false)
        {
            if (insensitive)
            {
                name = name.ToLower();
                filters.Add(comp => comp.gameObject.name.ToLower().Contains(name));
            }
            else
            {
                filters.Add(comp => comp.gameObject.name.Contains(name));
            }
            return this;
        }

        public T Execute()
        {
            if (filters.Count == 0)
            {
                return GetComponent();
            }
            else
            {
                var result = ExecuteMultiple();
                return result.Count > 0 ? result[0] : null; 
            }
        }

        public List<T> ExecuteMultiple()
        {
            if (filters.Count == 0)
            {
                return new(GetComponents());
            }
            else
            {
                var components = GetComponents();
                var results = new List<T>();
                foreach(var component in components)
                {
                    bool passed = true;
                    foreach(var filter in filters)
                    {
                        if(!filter(component))
                        {
                            passed = false;
                            break;
                        }
                    }
                    if (passed)
                    {
                        results.Add(component);
                    }
                }
                return results;
            }
        }

        T GetComponent()
        {
            return direction == Direction.Self
                ? monobehavior.GetComponent<T>()
                : direction == Direction.Parents
                ? monobehavior.GetComponentInParent<T>()
                : direction == Direction.Children
                ? monobehavior.GetComponentInChildren<T>()
                : Object.FindObjectOfType<T>();
        }

        T[] GetComponents()
        {
            return direction == Direction.Self
                ? monobehavior.GetComponents<T>()
                : direction == Direction.Parents
                ? monobehavior.GetComponentsInParent<T>()
                : direction == Direction.Children
                ? monobehavior.GetComponentsInChildren<T>()
                : Object.FindObjectsOfType<T>();
        }
    };
    public static FindQuery<T> Query<T>(this MonoBehaviour mb)
        where T : Component
    {
        return new(mb);
    }

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
