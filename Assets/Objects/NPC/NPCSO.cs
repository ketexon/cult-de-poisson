using UnityEngine;

[CreateAssetMenu(fileName = "NPC", menuName = "NPC")]
public class NPCSO : ScriptableObject
{
    public string Name;
    public string YarnStartNode; //this can be updated as the state of the game changes

    void Reset()
    {
        Name = "NPC Name";
        YarnStartNode = "Start";
    }
}
