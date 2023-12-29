using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Serialization.Json;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(SaveStateSO))]
public class SaveStateSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var baseDir = Application.persistentDataPath;
        var saveDir = serializedObject.FindProperty("saveDir").stringValue;
        EditorGUILayout.LabelField($"Save Path: {Path.Combine(baseDir, saveDir)}");
    }
}

#endif

[CreateAssetMenu(fileName = "SaveState", menuName = "State/SaveState")]
public class SaveStateSO : ScriptableObject
{
    [Tooltip("File path. Use \"{0}\" in filepath to represent the save slot index")]
    [SerializeField] string saveDir = "./savedata";
    [SerializeField] string filepath = "./{0}.json";
    [SerializeField] List<SavableSO> savables = new();

    public string AbsoluteSaveDir => Path.Combine(Application.persistentDataPath, saveDir);
    public string AbsoluteFilePath => Path.Combine(Application.persistentDataPath, saveDir, filepath);

    Dictionary<string, SavableSO> savableDict = new();

    void OnEnable()
    {
        foreach(var savable in savables)
        {
            savableDict[savable.Key] = savable;
        }
    }

    public int? MaxSaveSlot()
    {
        int? max = null;
        if (Directory.Exists(AbsoluteSaveDir))
        {
            foreach(var entry in Directory.EnumerateFileSystemEntries(saveDir)){
                Debug.Log(entry);
            }
        }
        return max;
    }

    public int NextSaveSlot()
    {
        var maxSaveSlot = MaxSaveSlot();
        return maxSaveSlot.HasValue ? maxSaveSlot.Value + 1 : 0;
    }

    public void Save(int slot)
    {
        string path = Path.Combine(AbsoluteSaveDir, filepath);
        using (StreamWriter sw = File.CreateText(path))
        {
            sw.Write(JsonSerialization.ToJson(savables));
        }
    }

    public void Load(int slot)
    {
        string path = Path.Join(AbsoluteSaveDir, filepath);
        using (StreamReader sr = File.OpenText(path))
        {
            JsonSerialization.FromJsonOverride(sr.ReadToEnd(), ref savables);
        }
    }
}
