using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ExtractTreeCollidersFromTerrain))]
public class ExtractTreeCollidersFromTerrainEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        GUILayout.Label("HELLO");

        if (GUILayout.Button("Bake Trees"))
        {
            (target as ExtractTreeCollidersFromTerrain).ExtractBakeDelete();
        }
    }
}