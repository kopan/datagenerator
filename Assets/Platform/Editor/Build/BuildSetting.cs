#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Xml; 
using System.Collections; 

public class ServerInfo
{
    public string ProductName;
    public string BundleName;
    public string ServerURL;
    public string PatchURL;
    public string PlatformType;
};

public class BuildSetting
{
    public  string      BundleVersion;
    
    public string       AndroidVersionCode;
    public string       IosVersionNaumber;

    public string       NchantAndroidKeyStoreName;
    public string       NchantAndroidKeyStorePassword;
    public string       NchantAndroidKeyAlias;
    public string       NchantAndroidKeyPassword;

    public ServerInfo DevServer;
    public ServerInfo AlphaServer;
    public ServerInfo BetaServer;
    public ServerInfo ReviewServer;
    public ServerInfo LiveServer;

    public string       AppleStoreURL;
    public string       AndroidStoreURL;

    public string       MaintainceUrl = "http://d3gujlus6nlhl6.cloudfront.net/Web/Maintain.html";
    public string       NoticeUrl = "http://d3gujlus6nlhl6.cloudfront.net/Web/Notice.html";

    public string       nchant_CSUrl = "support@nchant.co.kr";
    public string       nchant_CafeUrl = "http://www.mobirum.com/article/list?bbsId=1814&cafeId=wantedkiller&sort=DATE";

    public bool IsReady = false;

	public void Load()
    {
        if (IsReady == false)
        {
            DevServer = new ServerInfo();
            AlphaServer = new ServerInfo();
            BetaServer = new ServerInfo();
            ReviewServer = new ServerInfo();
            LiveServer = new ServerInfo();

            IsReady = true;
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(Application.dataPath + "/Platform/Editor/Build/Setting.xml");

        foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
        {
            if (node.Attributes == null) continue;

            // dev
            if (node.Attributes[0].Name == "productname_dev") DevServer.ProductName = node.Attributes[0].Value;
#if UNITY_ANDROID
            else if (node.Attributes[0].Name == "android_bundlename_dev") DevServer.BundleName =  node.Attributes[0].Value;
#elif UNITY_IPHONE
            else if (node.Attributes[0].Name == "ios_bundlename_dev") DevServer.BundleName = node.Attributes[0].Value;
#endif
            else if (node.Attributes[0].Name == "patch_url_dev")  DevServer.PatchURL = node.Attributes[0].Value;
            else if (node.Attributes[0].Name == "server_url_dev")  DevServer.ServerURL = node.Attributes[0].Value;
            else if (node.Attributes[0].Name == "platform_dev")  DevServer.PlatformType =  node.Attributes[0].Value;

            // alpha
            else if (node.Attributes[0].Name == "productname_alpha") AlphaServer.ProductName =  node.Attributes[0].Value;
#if UNITY_ANDROID
            else if (node.Attributes[0].Name == "android_bundlename_alpha") AlphaServer.BundleName = node.Attributes[0].Value;
#elif UNITY_IPHONE
            else if (node.Attributes[0].Name == "ios_bundlename_alpha") AlphaServer.BundleName = node.Attributes[0].Value;
#endif
            else if (node.Attributes[0].Name == "patch_url_alpha") AlphaServer.PatchURL = node.Attributes[0].Value;
            else if (node.Attributes[0].Name == "server_url_alpha") AlphaServer.ServerURL = node.Attributes[0].Value;
            else if (node.Attributes[0].Name == "platform_alpha") AlphaServer.PlatformType = node.Attributes[0].Value;

            // beta
           else if (node.Attributes[0].Name == "productname_beta") BetaServer.ProductName = node.Attributes[0].Value;
#if UNITY_ANDROID
            else if (node.Attributes[0].Name == "android_bundlename_beta") BetaServer.BundleName =  node.Attributes[0].Value;
#elif UNITY_IPHONE
            else if (node.Attributes[0].Name == "ios_bundlename_beta") BetaServer.BundleName =  node.Attributes[0].Value;
#endif
            else if (node.Attributes[0].Name == "patch_url_beta") BetaServer.PatchURL = node.Attributes[0].Value;
            else if (node.Attributes[0].Name == "server_url_beta") BetaServer.ServerURL = node.Attributes[0].Value;
            else if (node.Attributes[0].Name == "platform_beta") BetaServer.PlatformType = node.Attributes[0].Value;

            // riview
            else if (node.Attributes[0].Name == "productname_review") ReviewServer.ProductName = node.Attributes[0].Value;
#if UNITY_ANDROID
            else if (node.Attributes[0].Name == "android_bundlename_review") ReviewServer.BundleName = node.Attributes[0].Value;
#elif UNITY_IPHONE
            else if (node.Attributes[0].Name == "ios_bundlename_review") ReviewServer.BundleName =  node.Attributes[0].Value;
#endif
            else if (node.Attributes[0].Name == "patch_url_review") ReviewServer.PatchURL = node.Attributes[0].Value;
            else if (node.Attributes[0].Name == "server_url_review") ReviewServer.ServerURL = node.Attributes[0].Value;
            else if (node.Attributes[0].Name == "platform_review") ReviewServer.PlatformType = node.Attributes[0].Value;

            // live
            else if (node.Attributes[0].Name == "productname_live") LiveServer.ProductName = node.Attributes[0].Value;
#if UNITY_ANDROID
            else if (node.Attributes[0].Name == "android_bundlename_live") LiveServer.BundleName = node.Attributes[0].Value;
#elif UNITY_IPHONE
            else if (node.Attributes[0].Name == "ios_bundlename_live") LiveServer.BundleName =  node.Attributes[0].Value;
#endif
            else if (node.Attributes[0].Name == "patch_url_live") LiveServer.PatchURL = node.Attributes[0].Value;
            else if (node.Attributes[0].Name == "server_url_live") LiveServer.ServerURL = node.Attributes[0].Value;
            else if (node.Attributes[0].Name == "platform_live") LiveServer.PlatformType = node.Attributes[0].Value;

            // anroid
#if UNITY_ANDROID
            else if (node.Attributes[0].Name == "nchant_android_key_store_name") NchantAndroidKeyStoreName = Application.dataPath + "/../" + node.Attributes[0].Value;
            else if (node.Attributes[0].Name == "nchant_android_key_store_password") NchantAndroidKeyStorePassword = node.Attributes[0].Value;
            else if (node.Attributes[0].Name == "nchant_android_key_alias") NchantAndroidKeyAlias = node.Attributes[0].Value;
            else if (node.Attributes[0].Name == "nchant_android_key_password") NchantAndroidKeyPassword = node.Attributes[0].Value;

            // ios
#elif UNITY_IPHONE
#endif
            // common
            else if (node.Attributes[0].Name == "apple_store_url")  AppleStoreURL =  node.Attributes[0].Value;
            else if (node.Attributes[0].Name == "android_store_url") AndroidStoreURL =  node.Attributes[0].Value;
            else if (node.Attributes[0].Name == "maintenance_url") MaintainceUrl = node.Attributes[0].Value;
            else if (node.Attributes[0].Name == "notice_url") NoticeUrl = node.Attributes[0].Value;
            else if (node.Attributes[0].Name == "nchant_cs_url") nchant_CSUrl = node.Attributes[0].Value;
            else if (node.Attributes[0].Name == "nchant_cafe_url") nchant_CafeUrl = node.Attributes[0].Value;
        }
    }
}

#endif