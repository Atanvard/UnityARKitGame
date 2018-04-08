using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using SKStudios.Common.Editor;
using UnityEditor.Callbacks;

namespace SKStudios.Portals.Editor
{
    /// <summary>
    /// Scans for if the project has been properly configured. Opens a StartupPrompt if it has not been.
    /// </summary>
[InitializeOnLoad]
public class StartupScanner : ScriptableObject
{
    [DidReloadScripts]
        static StartupScanner () {
        /*
        TextAsset fileContents = Resources.Load<TextAsset>("VersionInfo");
        StartupPrompt.fileContents = fileContents.ToString().Split(System.Environment.NewLine.ToCharArray());*/
        //if (StartupPrompt.fileContents[0] == "0") {
	    string path = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
	    if (path == null)
	        return;

	    path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar));
	    string VersionInfoPath =/* path + */ @"VersionInfo.txt";
        if (!File.Exists(VersionInfoPath)) {
        //        SettingsWindow.Show();
            }
        //AssetDatabase.importPackageCompleted += RequireComponentDetector.DelayedScan;
	}

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void DidScriptsReload()
    {
        //AssetDatabase.importPackageCompleted += RequireComponentDetector.DelayedScan;
    }
}
 
}
