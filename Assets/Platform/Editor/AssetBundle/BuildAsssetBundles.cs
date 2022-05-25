using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildAsssetBundles : MonoBehaviour {
    
    [MenuItem("Tools/Build AssetBundles")] static void BuildAllAssetBundles()
    {
        var path = "Assets/AssetBundles";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        var manifest = BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.Android);

        EditorUtility.DisplayDialog("create", "Success!!!", "ok");
    }


}
