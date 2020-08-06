#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

[CreateAssetMenu(menuName = "Custom 2D/PSBer", fileName = "PSBer")]
public class PSBer : ScriptableObject
{
    public bool refresher;

    private static string GetPreFolder()
    {
        string dp = Application.dataPath;
        return dp.Remove(dp.Length - 6, 6);
    }

    private void OnValidate()
    {
        refresher = false;

        var folder = GetPreFolder() + AssetDatabase.GetAssetPath(this);
        folder = Path.GetDirectoryName(folder);

        var files = Directory.GetFiles(folder, "*.psd", SearchOption.AllDirectories);

        bool updated = false;

        foreach (var f in files)
        {
            if (f.ToLower().EndsWith(".psd"))
            {
                updated = true;

                var dest = f.Substring(0, f.Length - 3) + "psb";
                if (File.Exists(dest))
                    File.Delete(dest);

                File.Copy(f, dest); //Move
            }
        }

        if (updated)
        {
            UnityEditor.EditorApplication.update += DoRefresh;
        }
    }

    private void DoRefresh()
    {
        AssetDatabase.Refresh(); //I think doing this in OnValidate crashes the editor

        UnityEditor.EditorApplication.update -= DoRefresh;
    }
}
#endif