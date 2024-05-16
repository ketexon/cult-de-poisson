using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(LODGroupChildAssigner))]
public class LODGroupChildAssignerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if(GUILayout.Button("Set LOD group 0"))
        {
            var mb = target as MonoBehaviour;
            var lodGroup = mb.GetComponent<LODGroup>();
            var lods = lodGroup.GetLODs();
            if(lods.Length == 0)
            {
                Debug.LogError("Cannot set LODs when no LOD layers set.");
                return;
            }
            lods[0].renderers = mb.GetComponentsInChildren<Renderer>();
            lodGroup.SetLODs(lods);
        }
    }
}

#endif

public class LODGroupChildAssigner : MonoBehaviour
{
}
