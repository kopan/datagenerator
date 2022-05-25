
#if UNITY_EDITOR

using UnityEngine;

using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IPHONE
using UnityEditor.iOS.Xcode;
#endif

using System;
using System.IO;
using System.Collections;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;

public class PostBuild
{
    private static Thread m_Thread = null;
    private static bool m_bPostBuildFinish = false;

    private static readonly string ASSEMBLY_NAME = "Assembly-CSharp.dll";
    private static readonly string IL2CPPLIB_NAME = "libil2cpp.so";

    private static string BUILD_EXTRACT_FOLDER_PATH;
    private static string HASH_ASSET_FILE_PATH;

    private static string HASH_KEY;

    [PostProcessSceneAttribute(1000)]
	private static void PostSceneBuildCallback()
	{
        if(Application.isPlaying) return;

#if UNITY_ANDROID
        PostProcessSceneforAndroid();
#elif UNITY_IPHONE
        PostProcessSceneforIOS();
#endif
    }

    [PostProcessBuildAttribute(10000)]
    private static void PostProcessBuildCallback(BuildTarget buildTarget, string buildPath)
    {
        if(Application.isPlaying) return;

        m_Thread = null;
        m_bPostBuildFinish = false;

#if UNITY_ANDROID
#elif UNITY_IPHONE
        PostProcessBuildforIOS(buildPath);
#endif
    }

    static void ModifyAndroidManifest()
    {
    }

    static void PostProcessSceneforAndroid()
    {
        ScriptingImplementation buildBackend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);

        if(buildBackend == ScriptingImplementation.Mono2x)
        {
            if (m_bPostBuildFinish) return;

            UnityEngine.Debug.Log("Start mono2x build observer for android");

            // read Assembly and write hash
            EditorApplication.LockReloadAssemblies();

            HASH_ASSET_FILE_PATH = Application.dataPath + "/Plugins/Android/assets/verify";
            HASH_KEY = PlayerSettings.bundleVersion;

            string binPath = findAssemblyPath(ASSEMBLY_NAME);
            if(binPath != "" ) 
            {
                writeHash((int)ScriptingImplementation.Mono2x, binPath);
                m_bPostBuildFinish = true;
            }

            EditorApplication.UnlockReloadAssemblies();

            AssetDatabase.Refresh();
        }

        else if(buildBackend == ScriptingImplementation.IL2CPP)
        {
            if ( m_Thread != null) return;

            UnityEngine.Debug.Log("Start il2cpp build observer for android");

            // read libil2cpp.so and write hash        
            BUILD_EXTRACT_FOLDER_PATH = "Temp/StagingArea/libs/armeabi-v7a";

            HASH_ASSET_FILE_PATH = "Temp/StagingArea/assets/verify";
            HASH_KEY = PlayerSettings.bundleVersion;

            m_Thread = new Thread(Detection);
            m_Thread.Start();
        }
    }

    static void PostProcessSceneforIOS()
    {
        EditorApplication.LockReloadAssemblies();

        EditorApplication.UnlockReloadAssemblies();

        AssetDatabase.Refresh();
    }

    static void PostProcessBuildforIOS(string path)
    {
#if UNITY_IPHONE
        string projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
		string infoPath = path + "/Info.plist";

        PBXProject pbxProject = new PBXProject();
        pbxProject.ReadFromFile(projectPath);

        PlistDocument plist = new PlistDocument();
		plist.ReadFromFile(infoPath);

        string target = pbxProject.TargetGuidByName("Unity-iPhone");
      
        //Required Frameworks
        pbxProject.AddFrameworkToProject (target, "AssetsLibrary.framework", false);    // Album
        pbxProject.AddFrameworkToProject (target, "AddressBook.framework", false);      // Address
        pbxProject.AddFrameworkToProject (target, "CoreTelephony.framework", false);    // Telephone         
        pbxProject.AddFrameworkToProject (target, "AdSupport.framework", false);        // Access the advertising
            
        pbxProject.AddFrameworkToProject (target, "Security.framework", false);
        pbxProject.AddFrameworkToProject (target, "StoreKit.framework", false);
        pbxProject.AddFrameworkToProject (target, "GameKit.framework", false);
            
        pbxProject.AddBuildProperty(target, "OTHER_LDFLAGS", "-all_load");
        pbxProject.AddBuildProperty(target, "OTHER_LDFLAGS", "-lz");
        pbxProject.AddBuildProperty(target, "OTHER_LDFLAGS", "-lxml2");

        //plist.root["UIRequiredDeviceCapabilities"].AsArray ().AddString("gamekit");
		plist.root.SetString("UIViewControllerBasedStatusBarAppearance", "false");
        plist.root.SetString("ITSAppUsesNonExemptEncryption", "false");
          
        plist.root.SetString("NSCalendarsUsageDescription", "For customer service use only");
		plist.root.SetString("NSCameraUsageDescription", "For customer service use only");
		plist.root.SetString("NSPhotoLibraryUsageDescription", "For customer service use only");
				
		// url scheme
		PlistElementArray urlTypeArray = null;
		if (plist.root.values.ContainsKey("CFBundleURLTypes") == false)				
			urlTypeArray = plist.root.CreateArray("CFBundleURLTypes");
		else
			urlTypeArray = plist.root.values["CFBundleURLTypes"].AsArray();

		var urlTypeDict = urlTypeArray.AddDict();
		//urlTypeDict.SetString("CFBundleTypeRole", "Editor");
		urlTypeDict.SetString("CFBundleURLName", PlayerSettings.applicationIdentifier);

		string schemeName = (PlayerSettings.applicationIdentifier.Replace("com.","")).Replace(".","-") + "-1217";
		var urlScheme = urlTypeDict.CreateArray("CFBundleURLSchemes");
		urlScheme.AddString(schemeName);

        /*
		var locallization = plist.root.CreateArray ("CFBundleLocalizations");
		locallization.AddString ("en");
		locallization.AddString ("ja");
		locallization.AddString ("ko");
        */

        pbxProject.AddFrameworkToProject (target, "EventKit.framework", false);
        pbxProject.AddFrameworkToProject (target, "EventKitUI.framework", false);
        pbxProject.AddFrameworkToProject (target, "MessageUI.framework", false);

		plist.WriteToFile(infoPath);
		pbxProject.WriteToFile(projectPath);

#endif
    }

    static private void Detection()
    {
        string binPath = findLibPath(IL2CPPLIB_NAME);
        while (binPath == "")
        {
            Thread.Sleep(500);
            binPath = findLibPath(IL2CPPLIB_NAME);
        }
        Thread.Sleep(1000);
        writeHash((int)ScriptingImplementation.IL2CPP,binPath);
    }

    static string findAssemblyPath(string suffix)
    {
        foreach (System.Reflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                if (assembly.Location != null && 
                    !assembly.Location.Equals(string.Empty) &&
                    assembly.Location.EndsWith(suffix) )
                {
                    UnityEngine.Debug.Log("Get dll hashfile(" + assembly.Location + ") Success!");
                    return assembly.Location;
                }
            }
            catch (System.NotSupportedException)
            {
                 return "";
            }
        }

        return "";
    }

    static string findLibPath(string suffix)
    {
        try 
        {
            string[] files = Directory.GetFiles(BUILD_EXTRACT_FOLDER_PATH, "*.so", SearchOption.AllDirectories);
            foreach (string fileName in files)
            {
                if (fileName.EndsWith(suffix))
                {
                    UnityEngine.Debug.Log("Get lib hashfile(" + fileName + ") Success!");
                    return fileName;
                }
            }
        }
        catch (System.IO.IOException)
        {
            return "";
        }

        return "";
    }

    static void writeHash(int type, string srcFilePath)
    {
        UnityEngine.Debug.Log("Ready verifysign extraction!");

        FileStream binStream = new FileStream(srcFilePath, FileMode.Open, FileAccess.Read);

        byte[] binhash = MD5.Create().ComputeHash(binStream);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < binhash.Length; i++)
            sb.Append(binhash[i].ToString("x2"));

        binStream.Close();

        string verifysign = type + "," + sb.ToString();

        UnityEngine.Debug.Log("Verifysign(" + verifysign + ") extraction successful!");

        FileStream fileStream = new FileStream(HASH_ASSET_FILE_PATH, FileMode.Create);

        byte[] Key = new byte[32];
        byte[] IV = new byte[16];
        byte[] FileKey = System.Text.Encoding.UTF8.GetBytes(HASH_KEY);
        Array.Clear(Key, 0, 32);
        Array.Copy(FileKey, Key, (FileKey.Length > 32) ? 32 : FileKey.Length);
        Array.Copy(Key, IV, 16);

        RijndaelManaged aes = new RijndaelManaged();
        ICryptoTransform cryptoTransform = aes.CreateEncryptor(Key, IV);
        CryptoStream cryptoStream = new CryptoStream(fileStream, cryptoTransform, CryptoStreamMode.Write);
        cryptoStream.Write(System.Text.Encoding.UTF8.GetBytes(verifysign), 0, System.Text.Encoding.UTF8.GetBytes(verifysign).Length);
        cryptoStream.FlushFinalBlock();

        fileStream.Close();
        cryptoStream.Close();

        UnityEngine.Debug.Log("Verifysign write success!");
    }
}

#endif