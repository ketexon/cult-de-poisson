using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SavableSO : ScriptableObject
{
    abstract public string Key { get; }
}
