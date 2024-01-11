using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class InputUIElement : MonoBehaviour
{
    [SerializeField] Color activeColor = Color.black;
    [SerializeField] Color disabledColor = Color.gray;

    TMP_Text tmp;

    void Awake()
    {
        tmp = GetComponent<TMP_Text>();
    }

    public void Initialize(string key, string action, bool disabled)
    {
        tmp.text = $"{key}: {action}";
        tmp.color = disabled ? disabledColor : activeColor;
    }
}
