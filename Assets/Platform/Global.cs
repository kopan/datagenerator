using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviour
{
    public static string FileUri
    {
        // windows 의 경우, "file:///C:/Windows/..." 형태가 되어야 하고, persistantDataPath 가 "C:/" 부터 시작하므로 file:/// 으로 시작하는 게 맞고
        // 기타 안드로이드, ios의 경우 persistantDataPath가 "/mnt/" 같은 형태로 시작하고 "file:///mnt/..." 형태가 되어야하므로 file://으로 시작하는 게 맞음
        get
        {
            return "file:///";
        }
    }

    public static string StreamingAssetsPathForFile
    {
        get
        {
            if (Application.isEditor)
                return Application.streamingAssetsPath;
            else
            {
                string url = string.Empty;
                if (Application.streamingAssetsPath.Contains("://"))
                    url = Application.streamingAssetsPath;
                else
                    url = string.Concat(Global.FileUri, Application.streamingAssetsPath);

                return url;
            }

        }
    }
}
