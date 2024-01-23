#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class CommandPromptEditorUtil
{
    [MenuItem("Assets/Open in terminal")]
    public static void OpenInTerminal()
    {
        Object activeObject = Selection.activeObject;
        string path = AssetDatabase.GetAssetPath(activeObject);
        // Application.dataPath include Assets, but path does too
        var rootPath = Directory.GetParent(Application.dataPath).FullName;
        var assetPath = Path.Combine(rootPath, path);
        string directoryPath = Directory.Exists(assetPath) 
            ? assetPath 
            : Directory.GetParent(assetPath).FullName;

        directoryPath = Path.GetFullPath(directoryPath);

        Debug.Log(directoryPath);

#if UNITY_EDITOR_WIN
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            WorkingDirectory = directoryPath,
            FileName = "cmd.exe",
        });
#else
        Debug.LogError("Cannot open console on your platform: not supported");
#endif
    }
}
#endif