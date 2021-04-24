#if UNITY_EDITOR
using UnityEngine;
using System.IO;
using UnityEditor;

[CreateAssetMenu(menuName = "Custom 2D/PSBer", fileName = "PSBer")]
public class PSBer : ScriptableObject
{
    //Fields
    private const int UPDATE_INTERVAL = 20;

    [Space(30)]
    public bool refresher;
    [Space(30)]
    [Tooltip("If this is false, you generally have to click on \"Refresher\" every time")]
    public bool constantlyUpdate;
    [Tooltip("If the .psd file is an exported file, such as an Affinity Designer slice")]
    public bool deleteSource;
    public bool debug;



    //Methods
    private void UpdateDo()
    {
        if (Time.frameCount % UPDATE_INTERVAL == 0 && !Application.isPlaying && UnityEditorInternal.InternalEditorUtility.isApplicationActive)
            Do();
    }
    private void Do()
    {
        string folder = Application.dataPath;
        folder = Path.GetDirectoryName(folder.Remove(folder.Length - 6, 6) + AssetDatabase.GetAssetPath(this));

        var files = Directory.GetFiles(folder, "*.psd", SearchOption.AllDirectories);

        bool updatedFiles = false;

        foreach (var fromPath in files)
        {
            if (fromPath.ToLowerInvariant().EndsWith(".psd"))
            {
                bool copy = true;

                var toPath = fromPath.Substring(0, fromPath.Length - 3) + "psb";
                if (File.Exists(toPath))
                {
                    copy = new FileInfo(fromPath).LastWriteTime > new FileInfo(toPath).LastWriteTime;
                    if (copy)
                        File.Delete(toPath);
                }

                if (copy)
                {
                    updatedFiles = true;
                    if (deleteSource)
                    {
                        if (debug)
                            Debug.Log("Renamed (" + fromPath + ") to (" + toPath + ")");

                        File.Move(fromPath, toPath);
                    }
                    else
                    {
                        if (debug)
                            Debug.Log("Copied from (" + fromPath + ") to (" + toPath + ")");

                        File.Copy(fromPath, toPath);
                    }
                }
            }
        }

        if (updatedFiles)
        {
            EditorApplication.delayCall += () => { AssetDatabase.Refresh(); }; //I think doing this in OnValidate can crash the editor
        }
    }

    private void AddListener()
    {
        EditorApplication.update -= UpdateDo;
        if (constantlyUpdate)
            EditorApplication.update += UpdateDo;
    }



    //Lifecycle
    [InitializeOnLoadMethod]
    private static void Init()
    {
        var psbers = AssetDatabase.FindAssets("t:PSBer");
        for (int i = 0; i < psbers.Length; i++)
        {
            var psber = AssetDatabase.LoadAssetAtPath<PSBer>(AssetDatabase.GUIDToAssetPath(psbers[i]));
            if (psber)
                psber.AddListener();
        }
    }

    private void OnValidate()
    {
        refresher = false;

        Do();

        AddListener();
    }
}
#endif