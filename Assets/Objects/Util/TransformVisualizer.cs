using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class TransformVisualizer : MonoBehaviour
{
#if UNITY_EDITOR
    Mesh gizmosMesh;

    void Awake()
    {
        gizmosMesh = new() { name = "Bucket Fish Spawn Mesh" };

        Vector3 offset = Vector3.one / -2;

        Vector3[] vertices = {
            new Vector3 (0, 0, 0) + offset,
            new Vector3 (1, 0, 0) + offset,
            new Vector3 (1, 1, 0) + offset,
            new Vector3 (0, 1, 0) + offset,
            new Vector3 (0, 1, 1) + offset,
            new Vector3 (1, 1, 1) + offset,
            new Vector3 (1, 0, 1) + offset,
            new Vector3 (0, 0, 1) + offset,
        };

        int[] indices = {
            0, 2, 1, //face front
	        0, 3, 2,
            2, 3, 4, //face top
	        2, 4, 5,
            1, 2, 5, //face right
	        1, 5, 6,
            0, 7, 4, //face left
	        0, 4, 3,
            5, 4, 7, //face back
	        5, 7, 6,
            0, 6, 7, //face bottom
	        0, 1, 6
        };

        gizmosMesh.SetVertices(vertices);
        gizmosMesh.SetIndices(indices, MeshTopology.Triangles, 0);
        gizmosMesh.Optimize();
        gizmosMesh.RecalculateNormals();
    }
#endif

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Gizmos.DrawWireMesh(
            gizmosMesh, 0, 
            position: transform.position, 
            rotation: transform.rotation, 
            scale: transform.localScale
        );
#endif
    }
}
