using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum QuestType
{
    Normal,
    Satellite
}

[CreateAssetMenu(fileName = "Quest", menuName = "State/Quest")]
public class QuestSO : ScriptableObject
{
    public string Name = string.Empty;
    public QuestType Type = QuestType.Normal;
    public bool MustStartToComplete = false;
    public List<QuestSO> Subquests = new();
}
