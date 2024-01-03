using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonBehaviour<T> : MonoBehaviour
    where T : SingletonBehaviour<T>
{
    public static T Instance { get; private set; }

    virtual protected bool DestroyNewInstance => true;

    virtual protected void Awake()
    {
        if (Instance)
        {
            if (DestroyNewInstance)
            {
                Destroy(gameObject);
            }
            else
            {
                Destroy(Instance.gameObject);
            }
        }
        Instance = this as T;
    }
}