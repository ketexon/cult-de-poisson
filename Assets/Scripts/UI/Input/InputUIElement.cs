using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class InputUIElement : MonoBehaviour
{
    public void Initialize(string key, string action)
    {
        GetComponent<TMP_Text>().text = $"{key}: {action}";
    }
}
