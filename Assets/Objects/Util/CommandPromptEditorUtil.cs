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

#if UNITY_EDITOR_WIN
        // try to open Windows Terminal
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "wt.exe",
                Arguments = @$"-d {'"'}{directoryPath}{'"'}",
            });
            return;
        }
        catch(System.ComponentModel.Win32Exception)
        {}

        // try to open Powershell 7
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "pwsh.exe",
                WorkingDirectory = directoryPath,
            });
        }
        catch (System.ComponentModel.Win32Exception)
        { }

        // try to open Powershell 5
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell.exe",
                WorkingDirectory = directoryPath,
            });
        }
        catch (System.ComponentModel.Win32Exception)
        {}

        // try to open Command Prompt
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                WorkingDirectory = directoryPath,
            });
        }
        catch (System.ComponentModel.Win32Exception)
        {}

        throw new System.NotSupportedException("Could not open terminal");
#else
        Debug.LogError("Cannot open console on your platform: not supported");
#endif
    }
}
#endif