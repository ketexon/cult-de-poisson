using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(PlayerInventorySO))]
[CanEditMultipleObjects]
class PlayerInventorySOEditor : Editor
{
    bool currentFishFoldedOut = true;

    void OnEnable()
    {
        (target as PlayerInventorySO).FishAddedEvent += OnFishAdded;
    }

    void OnDisable()
    {
        (target as PlayerInventorySO).FishAddedEvent -= OnFishAdded;
    }

    void OnFishAdded(FishSO fish)
    {
        Repaint();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        currentFishFoldedOut = EditorGUILayout.BeginFoldoutHeaderGroup(
            currentFishFoldedOut,
            "Current fish"
        );

        if(currentFishFoldedOut)
        {
            EditorGUI.BeginDisabledGroup(true);
            ++EditorGUI.indentLevel;

            var inv = target as PlayerInventorySO;
            int i = 0;

            foreach (var fish in inv.Fish)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PrefixLabel($"Element {i++}");
                EditorGUILayout.ObjectField(
                    fish,
                    fish.GetType(),
                    false
                );

                EditorGUILayout.EndHorizontal();
            }

            --EditorGUI.indentLevel;
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}

#endif

[CreateAssetMenu(fileName = "Inventory", menuName = "State/Inventory")]
public class PlayerInventorySO : SavableSO
{
    public override string Key => "inventory";

    [SerializeField] public int MaxFish = 6;
    [SerializeField] public List<FishSO> StartingFish = new();

    [System.NonSerialized]
    List<FishSO> fish;

    public IReadOnlyList<FishSO> Fish => fish;

    public bool Full => Fish.Count >= MaxFish;

    public System.Action<FishSO> FishAddedEvent;
    public System.Action<FishSO> FishRemovedEvent;

    void OnEnable()
    {
        fish = new(StartingFish);
    }

    public void AddFish(FishSO fish)
    {
        this.fish.Add(fish);
        FishAddedEvent?.Invoke(fish);
    }

    public void RemoveFish(FishSO fish)
    {
        this.fish.Remove(fish);
        FishRemovedEvent?.Invoke(fish);
    }
}
