using UnityEngine;

using System;
using System.Xml;
using System.Net;
using System.IO;
using System.Text; 
using System.Collections;  
using System.Collections.Generic;

public class ConfigManager
{
    static bool IsLoad = false;

    public static string Platform = "";
    public static string PatchUrl = "";
    public static string ServerUrl = "";
    public static string StoreUrl = "";
    public static string MaintainceUrl = "";
    public static string NoticeUrl = "";
    public static string CSUrl = "";
    public static string CafeUrl = ""; 

    public static void Load()
    {
        if (IsLoad) return;

        try
        {
            UnityEngine.Object obj = Resources.Load("config/config");
            TextAsset textAsset = obj as TextAsset;
            string xmlContent = textAsset.text;
            byte[] byteArray = Encoding.UTF8.GetBytes(xmlContent);
            MemoryStream ms = new MemoryStream(byteArray);

            XmlDocument _xmldoc = new XmlDocument();
            _xmldoc.Load(ms);

            ReadXml(_xmldoc);

            IsLoad = true;
        }
        catch (XmlException ex)
        {
            Debug.Log("Config File XmlException Fail");
            Debug.Log(ex.Message);
        }

        catch (FileNotFoundException ex)
        {
            Debug.Log("Config File FileNotFoundException Fail");
            Debug.Log(ex.Message);
        }

        catch (DirectoryNotFoundException ex)
        {
            Debug.Log("Config File DirectoryNotFoundException Fail");
            Debug.Log(ex.Message);
        }
    }

    public static void LoadExternal()
    {
        try 
        { 
            string path = "";
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    path = Application.dataPath + "/../resources/config/config.xml";
                    break;
                default:
                    path = Application.persistentDataPath + "/resources/config/config.xml";
                    break;
            }

            XmlDocument _xmldoc = new XmlDocument();
            _xmldoc.Load(path);           
            ReadXml(_xmldoc);
        }
        catch (XmlException ex)
        {
            Debug.Log("Config File Exception Fail");
            Debug.Log(ex.Message);
        }

        catch (FileNotFoundException ex)
        {
            Debug.Log("Config File FileNotFoundException Fail");
            Debug.Log(ex.Message);
        }

        catch (DirectoryNotFoundException ex)
        {
            Debug.Log("Config File DirectoryNotFoundException Fail");
            Debug.Log(ex.Message);
        }
    }

    private static void ReadXml(XmlDocument _xmldoc)
    {
        XmlElement root = _xmldoc.DocumentElement;
        foreach (XmlNode node in root.ChildNodes)
        {
            if (null == node.Attributes) continue;

            if (0 == node.Attributes[0].Name.CompareTo("platform"))
                Platform = node.Attributes[0].Value;
            else if (0 == node.Attributes[0].Name.CompareTo("patch_url"))
                PatchUrl = node.Attributes[0].Value;
            else if (0 == node.Attributes[0].Name.CompareTo("server_url"))
                ServerUrl = node.Attributes[0].Value;
#if UNITY_IPHONE && !UNITY_EDITOR
            else if (0 == node.Attributes[0].Name.CompareTo("apple_store_url"))
                StoreUrl = node.Attributes[0].Value;
#elif UNITY_ANDROID && !UNITY_EDITOR
            else if (0 == node.Attributes[0].Name.CompareTo("android_store_url"))
                StoreUrl = node.Attributes[0].Value;
#endif
            else if (0 == node.Attributes[0].Name.CompareTo("maintenance_url"))
                MaintainceUrl = node.Attributes[0].Value;
            else if (0 == node.Attributes[0].Name.CompareTo("notice_url"))
                NoticeUrl = node.Attributes[0].Value;
            else if (0 == node.Attributes[0].Name.CompareTo("cs_url"))
                CSUrl = node.Attributes[0].Value;
            else if (0 == node.Attributes[0].Name.CompareTo("cafe_url"))
                CafeUrl = node.Attributes[0].Value;
        }
    }
}
