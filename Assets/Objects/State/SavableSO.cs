using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Serialization.Json;
using UnityEngine;

[System.Serializable]
public abstract class SavableSO : ScriptableObject
{
    abstract public string Key { get; }
}
