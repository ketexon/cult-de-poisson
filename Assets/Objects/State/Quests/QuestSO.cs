using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "State/Quest")]
public class QuestSO : ScriptableObject
{
    public string Name = string.Empty;
    public bool MustStartToComplete = false;
    public List<QuestSO> Subquests = new();
}
