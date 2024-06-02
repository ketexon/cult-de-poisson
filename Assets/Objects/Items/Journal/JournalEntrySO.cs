using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Journal Entry", menuName = "Journal Entry")]
public class JournalEntrySO : ScriptableObject
{
    [TextArea(3, 10)]
    [SerializeField] public string Text;
}
