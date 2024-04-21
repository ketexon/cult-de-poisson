using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "GameState", menuName = "State/GameState")]
public class GameStateSO : ScriptableObject
{
    public PlayerInventorySO Inventory;
    public QuestStateSO Quests;
}
