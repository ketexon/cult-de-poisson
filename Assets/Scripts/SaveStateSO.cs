using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Serialization.Json;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(SaveStateSO))]
public class SaveStateSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.LabelField($"Persistent Data Path: {Application.persistentDataPath}");
        if(GUILayout.Button("Reveal in Explorer"))
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }
    }
}

#endif

[CreateAssetMenu(fileName = "SaveState", menuName = "State/SaveState")]
public class SaveStateSO : ScriptableObject
{
    [Tooltip("File path. Use \"{0}\" in filepath to represent the save slot index")]
    [SerializeField] string saveDirRoot = "./savedata";
    [SerializeField] string saveDir = "./{0}";
    [SerializeField] string saveFilename = "{0}.json";
    [SerializeField] List<SavableSO> savables = new();

    int? lastSaveSlot = null;

    public string AbsoluteSaveDirRoot => Path.Combine(Application.persistentDataPath, saveDirRoot);
    public string AbsoluteSaveDir(int slot) => Path.Combine(Application.persistentDataPath, saveDirRoot, string.Format(saveDir, slot));

    public string AbsoluteSaveFile(int slot, string filename) => Path.Combine(AbsoluteSaveDir(slot), string.Format(saveFilename, filename));

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
        if (Directory.Exists(AbsoluteSaveDirRoot))
        {
            foreach(var entry in Directory.EnumerateFileSystemEntries(saveDir)){
                max = System.Math.Max(max.GetValueOrDefault(-1), int.Parse(Regex.Match(entry, @"\d+$").Value));
            }
        }
        return max;
    }

    public int NextSaveSlot()
    {
        var maxSaveSlot = MaxSaveSlot();
        return maxSaveSlot.HasValue ? maxSaveSlot.Value + 1 : 0;
    }

    void EnsureSaveDirectory(int slot)
    {
        if (!Directory.Exists(AbsoluteSaveDir(slot)))
        {
            Directory.CreateDirectory(AbsoluteSaveDir(slot));
        }
    }

    public async void Save()
    {
        Debug.Assert(lastSaveSlot.HasValue, "Can't save without save slot when haven't saved last");
        if (lastSaveSlot.HasValue)
        {
            await Save(lastSaveSlot.Value);
        }
    }

    public async Task Save(int slot)
    {
        throw new System.NotImplementedException();
        Debug.Log("HI");
        EnsureSaveDirectory(slot);

        foreach(var savable in savables)
        {
            using (StreamWriter sw = File.CreateText(AbsoluteSaveFile(slot, savable.Key)))
            {
                await sw.WriteAsync(JsonSerialization.ToJson(savable));
            }
        }
        
        lastSaveSlot = slot;
    }

    public async void Load(int slot)
    {
        throw new System.NotImplementedException();
        foreach (var savable in savables)
        {
            using (StreamReader sr = File.OpenText(AbsoluteSaveFile(slot, savable.Key)))
            {
                var savableTmp = savable;
                JsonSerialization.FromJsonOverride(await sr.ReadToEndAsync(), ref savableTmp);
            }
        }

        lastSaveSlot = slot;
    }
}
