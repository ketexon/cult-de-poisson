using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ExtractTreeCollidersFromTerrain))]
public class ExtractTreeCollidersFromTerrainEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Bake Trees"))
        {
            (target as ExtractTreeCollidersFromTerrain).ExtractBakeDelete();
        }
    }
}

#endif

/// <summary>
/// https://forum.unity.com/threads/navmeshsurface-and-terrain-trees.1295496/
/// </summary>
[RequireComponent(typeof(Terrain))]
[RequireComponent(typeof(NavMeshSurface))]
public class ExtractTreeCollidersFromTerrain : MonoBehaviour
{
    private readonly List<GameObject> _createdObjects = new List<GameObject>();

    private Terrain Terrain => GetTerrain();
    private Terrain _terrain;

    private Terrain GetTerrain()
    {
        if (_terrain != null)
            return _terrain;

        _terrain = GetComponent<Terrain>();
        return _terrain;
    }

    [ContextMenu("Extract, Bake, and Delete")]
    public void ExtractBakeDelete()
    {
        var count = ExtractTreesFromTerrain();
        GetComponent<NavMeshSurface>().BuildNavMesh();
        DestroyCachedObjects();
        Debug.Log($"<color=#99ff99>Successfully created {count} colliders and baked the NavMesh!</color>");
    }

    [ContextMenu("Delete Collider Objects")]
    private void DestroyCachedObjects()
    {
        foreach (var obj in _createdObjects)
            DestroyImmediate(obj);

        _createdObjects.Clear();
    }

    [ContextMenu("Extract Only")]
    public int ExtractTreesFromTerrain()
    {
        for (var prototypeIndex = 0; prototypeIndex < Terrain.terrainData.treePrototypes.Length; prototypeIndex++)
            ExtractInstancesFromTreePrototype(prototypeIndex);

        return _createdObjects.Count;
    }

    private void ExtractInstancesFromTreePrototype(int prototypeIndex)
    {
        var tree = Terrain.terrainData.treePrototypes[prototypeIndex];
        var instances = Terrain.terrainData.treeInstances.Where(x => x.prototypeIndex == prototypeIndex).ToArray();

        for (var instanceIndex = 0; instanceIndex < instances.Length; instanceIndex++)
        {
            UpdateInstancePosition(instances, instanceIndex);
            CreateNavMeshObstacle(tree, prototypeIndex, instances, instanceIndex);
        }
    }

    private void CreateNavMeshObstacle(TreePrototype tree, int prototypeIndex, TreeInstance[] instances, int instanceIndex)
    {
        if(!tree.prefab) return;

        var navMeshObstacle = tree.prefab.GetComponent<NavMeshObstacle>();
        if (!navMeshObstacle) return;

        var primitiveScale = CalculatePrimitiveScale(navMeshObstacle);

        var obj = CreateGameObjectForNavMeshObstacle(tree, instances, instanceIndex, primitiveScale);
        _createdObjects.Add(obj);
    }

    private GameObject CreateGameObjectForNavMeshObstacle(TreePrototype tree, TreeInstance[] instances, int instanceIndex, Vector3 primitiveScale)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        obj.name = tree.prefab.name + instanceIndex;
        obj.layer = Terrain.preserveTreePrototypeLayers ? tree.prefab.layer : Terrain.gameObject.layer;
        obj.transform.localScale = primitiveScale;
        obj.transform.position = instances[instanceIndex].position;
        obj.transform.parent = Terrain.transform;
        obj.isStatic = true;

        return obj;
    }

    private Vector3 CalculatePrimitiveScale(NavMeshObstacle navMeshObstacle)
    {
        if (navMeshObstacle.shape == NavMeshObstacleShape.Capsule)
            return navMeshObstacle.radius * Vector3.one;

        return navMeshObstacle.size;
    }

    private void UpdateInstancePosition(TreeInstance[] instances, int instanceIndex)
    {
        instances[instanceIndex].position = Vector3.Scale(instances[instanceIndex].position, Terrain.terrainData.size);
        instances[instanceIndex].position += Terrain.GetPosition();
    }
}
