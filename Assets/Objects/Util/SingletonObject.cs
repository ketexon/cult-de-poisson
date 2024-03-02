using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SingletonObject<T> : ScriptableObject
    where T : SingletonObject<T>
{
    public static T Instance { get; private set; }

    virtual protected void OnEnable()
    {
        if(Instance != null && Instance != this)
        {
            Debug.LogWarning($"Multiple instances of SingletonObject {typeof(T).Name}");
        }
        Instance = this as T;
    }

    virtual protected void OnDisable()
    {
        Instance = null;
    }
}