#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Xml;
using System.Linq; 

public class BuildWindow : EditorWindow
{
    static BuildWindow window = null;

    private static string BUILD_PATH;

    bool[] m_ServerType = new bool[5] { true, true, true, true, true };

    public enum ServerType
    {
        Dev,
        Alpha,
        Beta,
        Review,
        Live,
    }

    int m_nServerTypeMaxCount = 5;

    public enum Publisher
    {
        Self,       //자체 서비스
    }
    Publisher[] m_Platform = new Publisher[5] { Publisher.Self, Publisher.Self, Publisher.Self, Publisher.Self, Publisher.Self };

    bool m_bShowSetting = true;
    string  m_Status = "Idle";

    int         m_nSelectedServerCount = 0; 
    ServerType  m_eSelectedServer;
    Publisher   m_eSelectedPlatform = Publisher.Self;

	BuildSetting m_BuildSetting; 

    [MenuItem("Tools/Builder")]
    static public void Init()
    {
        // Get existing open window or if none, make a new one:
        window = (BuildWindow)EditorWindow.GetWindow(typeof(BuildWindow), false, "Builder", true);   
    }

    void OnEnable()
    {
#if UNITY_ANDROID
        BUILD_PATH = Path.GetFullPath(".") + "/../../Distribution";
#elif UNITY_IPHONE
        BUILD_PATH  = Path.GetFullPath(".") + "/../OProject_XCode"; 
#else
	    BUILD_PATH  = Path.GetFullPath(".");
#endif

        m_BuildSetting = new BuildSetting();
        m_BuildSetting.Load();
    }
		
    void OnGUI()
    {
        PlayerSettings.bundleVersion = EditorGUILayout.TextField("Bundle Version", PlayerSettings.bundleVersion);

        EditorGUILayout.Space();

        if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            PlayerSettings.Android.bundleVersionCode = Convert.ToInt32(EditorGUILayout.TextField("Bundle Version Code", PlayerSettings.Android.bundleVersionCode.ToString()));
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            PlayerSettings.iOS.buildNumber = EditorGUILayout.TextField("Bundle Version Number", PlayerSettings.iOS.buildNumber);
        else
        {
            EditorGUILayout.TextField("Fetal Error : Change Target Platform");
            return;
        }
          
        EditorGUILayout.Space();
        m_bShowSetting = EditorGUILayout.Foldout(m_bShowSetting, "Server Settings");
        if(m_bShowSetting)
        {
            EditorGUI.indentLevel++;

            m_nSelectedServerCount = 0;
            for(int i = 0; i< m_nServerTypeMaxCount; i++)
            {
                EditorGUILayout.BeginHorizontal();

                m_ServerType[i] = EditorPrefs.GetBool(Enum.GetName(typeof(ServerType), i));               
                m_ServerType[i] = EditorGUILayout.ToggleLeft(Enum.GetName(typeof(ServerType), i), m_ServerType[i], GUILayout.MaxWidth(120));                
                if (m_ServerType[i] != EditorPrefs.GetBool(Enum.GetName(typeof(ServerType), i)))
                    EditorPrefs.SetBool(Enum.GetName(typeof(ServerType), i), m_ServerType[i]);

                if (m_ServerType[i])
                {
#if UNITY_IPHONE
                    for (int j = 0; j < m_nServerTypeMaxCount; j++)
                    {
                        if (i == j) continue;

                        m_ServerType[j] = false;
                        if (m_ServerType[j] != EditorPrefs.GetBool(Enum.GetName(typeof(ServerType), j)))
                            EditorPrefs.SetBool(Enum.GetName(typeof(ServerType), j), m_ServerType[j]);
                    }
#endif

                    m_Platform[i] = (Publisher)EditorGUILayout.EnumPopup("", m_Platform[i]);

                    m_nSelectedServerCount++;
                    m_eSelectedServer = (ServerType)Enum.Parse(typeof(ServerType), Enum.GetName(typeof(ServerType), i));
                    m_eSelectedPlatform = m_Platform[i];
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        PlayerSettings.SetScriptingBackend(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), (ScriptingImplementation)EditorGUILayout.EnumPopup("Scripting Backend", PlayerSettings.GetScriptingBackend(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget))));

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Generate Status :", m_Status);
        //
        if (m_nSelectedServerCount == 1)
        {
            if (GUILayout.Button("Apply Settings") == true)
            {
                CopySplash(m_eSelectedServer, m_eSelectedPlatform);
                WriteConfig(m_eSelectedServer, m_eSelectedPlatform);
                ModifyAndroidSetting(m_eSelectedServer, m_eSelectedPlatform);
                ModifyProjectSetting(m_eSelectedServer, m_eSelectedPlatform);

                AssetDatabase.Refresh();

                m_Status = "idle";
            }

            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Build", GUILayout.Height(30)) == true)
        {
            Execute();
            if(m_Status == "Finish") m_Status = "Idle";
        }
    }

    void Execute()
    {
        m_Status = "Building...";

        if (Directory.Exists(BUILD_PATH) == false)
            Directory.CreateDirectory(BUILD_PATH);
        
        for (int i = 0; i <m_nServerTypeMaxCount; i++)
        {
            if(m_ServerType[i])
            {
                string serverName = Enum.GetName(typeof(ServerType),i);
                ServerType server = (ServerType)Enum.Parse(typeof(ServerType), serverName);

                // Splash Copy
                CopySplash(server, m_Platform[i]);

                // Config 수정 
                WriteConfig(server, m_Platform[i]);

                // Modify Android Settings
                ModifyAndroidSetting(server, m_Platform[i]);

                // Modify Build Settings 
                ModifyProjectSetting(server, m_Platform[i]);

                // Asset Refreash
                AssetDatabase.Refresh();
                 
                // Build
                Build(server, m_Platform[i], BUILD_PATH);
            } 
        }

        if (m_Status == "Building...") m_Status = "Finish";
    }


    bool CopyDirectory(string SrcPath, string DstPath , bool SrcDelete = true)
    {
        if (Directory.Exists(SrcPath))
        {
            if (!Directory.Exists(DstPath))
                Directory.CreateDirectory(DstPath);

            foreach (string dirPath in Directory.GetDirectories(SrcPath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(SrcPath, DstPath));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(SrcPath, "*.*",SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(SrcPath, DstPath), true);

            if(SrcDelete) Directory.Delete(SrcPath,true);

            return true;
        }

        return false;
    }

    void CopySplash(ServerType server, Publisher platformtype)
    {
    }

    void WriteConfig(ServerType server, Publisher platformtype)
    {
        m_BuildSetting.Load();

        if (!Directory.Exists(Application.dataPath + "/Resources/config"))
            Directory.CreateDirectory(Application.dataPath + "/Resources/config");

        var configFile = Application.dataPath + "/Resources/config/config.xml";
        XmlWriter Xmlwriter = XmlWriter.Create(configFile);

        Xmlwriter.WriteStartDocument();
        Xmlwriter.WriteWhitespace("\r\n");

        Xmlwriter.WriteComment(" " + server.ToString() + " Configuration ");
        Xmlwriter.WriteWhitespace("\r\n");

        Xmlwriter.WriteStartElement("Root");
        Xmlwriter.WriteWhitespace("\r\n");

        Xmlwriter.WriteStartElement("Element");  
        Xmlwriter.WriteAttributeString("platform", platformtype.ToString().ToLower());
        Xmlwriter.WriteEndElement();
        Xmlwriter.WriteWhitespace("\r\n");

        Xmlwriter.WriteStartElement("Element");

        if (server == ServerType.Dev)
            Xmlwriter.WriteAttributeString("patch_url", m_BuildSetting.DevServer.PatchURL);
        else if (server == ServerType.Alpha)
            Xmlwriter.WriteAttributeString("patch_url", m_BuildSetting.AlphaServer.PatchURL);
        else if (server == ServerType.Beta)
            Xmlwriter.WriteAttributeString("patch_url", m_BuildSetting.BetaServer.PatchURL);
        else if (server == ServerType.Review)
            Xmlwriter.WriteAttributeString("patch_url", m_BuildSetting.ReviewServer.PatchURL);
        else if (server == ServerType.Live)
            Xmlwriter.WriteAttributeString("patch_url", m_BuildSetting.LiveServer.PatchURL);
        else 
            Xmlwriter.WriteAttributeString("patch_url", "");

        Xmlwriter.WriteEndElement();
        Xmlwriter.WriteWhitespace("\r\n");


        Xmlwriter.WriteStartElement("Element");

        if (server == ServerType.Dev)
            Xmlwriter.WriteAttributeString("server_url", m_BuildSetting.DevServer.ServerURL );
        else if (server == ServerType.Alpha)
            Xmlwriter.WriteAttributeString("server_url", m_BuildSetting.AlphaServer.ServerURL);
        else if (server == ServerType.Beta)
            Xmlwriter.WriteAttributeString("server_url", m_BuildSetting.BetaServer.ServerURL);
        else if (server == ServerType.Review)
            Xmlwriter.WriteAttributeString("server_url", m_BuildSetting.ReviewServer.ServerURL);
        else if (server == ServerType.Live)
            Xmlwriter.WriteAttributeString("server_url", m_BuildSetting.LiveServer.ServerURL);
        else 
            Xmlwriter.WriteAttributeString("server_url", "");

        Xmlwriter.WriteEndElement();
        Xmlwriter.WriteWhitespace("\r\n");

        Xmlwriter.WriteStartElement("Element");
        Xmlwriter.WriteAttributeString("apple_store_url", m_BuildSetting.AppleStoreURL);
        Xmlwriter.WriteEndElement();
        Xmlwriter.WriteWhitespace("\r\n");

        Xmlwriter.WriteStartElement("Element");
        if (platformtype == Publisher.Self)
            Xmlwriter.WriteAttributeString("android_store_url", m_BuildSetting.AndroidStoreURL + m_BuildSetting.LiveServer.BundleName);
        else
            Xmlwriter.WriteAttributeString("android_store_url", m_BuildSetting.AndroidStoreURL + m_BuildSetting.LiveServer.BundleName);
        Xmlwriter.WriteEndElement();
        Xmlwriter.WriteWhitespace("\r\n");

        Xmlwriter.WriteStartElement("Element");
        Xmlwriter.WriteAttributeString("maintenance_url", m_BuildSetting.MaintainceUrl);
        Xmlwriter.WriteEndElement();
        Xmlwriter.WriteWhitespace("\r\n");

        Xmlwriter.WriteStartElement("Element");
        Xmlwriter.WriteAttributeString("notice_url", m_BuildSetting.NoticeUrl);
        Xmlwriter.WriteEndElement();
        Xmlwriter.WriteWhitespace("\r\n");

        Xmlwriter.WriteStartElement("Element");
        if (platformtype == Publisher.Self)
            Xmlwriter.WriteAttributeString("cs_url", m_BuildSetting.nchant_CSUrl);
        else
            Xmlwriter.WriteAttributeString("cs_url", "");
        Xmlwriter.WriteEndElement();
        Xmlwriter.WriteWhitespace("\r\n");

        Xmlwriter.WriteStartElement("Element");
        if (platformtype == Publisher.Self)
            Xmlwriter.WriteAttributeString("cafe_url", m_BuildSetting.nchant_CafeUrl);
        else 
            Xmlwriter.WriteAttributeString("cafe_url", "");
        Xmlwriter.WriteEndElement();
        Xmlwriter.WriteWhitespace("\r\n");

        Xmlwriter.WriteEndElement();
        Xmlwriter.WriteEndDocument();
        Xmlwriter.Close();

        var patchPath = Application.dataPath + "/../resources/config";
        if (!Directory.Exists(patchPath))
            Directory.CreateDirectory(patchPath);

        File.Copy(configFile, patchPath + "/config.xml", true);

        // delete config in patchfolder
        //if (File.Exists(Application.dataPath + patchPath + "/config.xml"))
        //    File.Delete(Application.dataPath + "/../resources/config/config.xml");

    }

    void ModifyAndroidSetting(ServerType server, Publisher platformtype)
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android) return;

        // Hash key setting
        if (!Directory.Exists(Application.dataPath + "/Plugins/Android/assets"))
            Directory.CreateDirectory(Application.dataPath + "/Plugins/Android/assets");

        /*
        // Plugin Setting -- 필요 없음
        if (!File.Exists(Application.dataPath + "/Plugins/Android/AndroidLocalNotification.jar"))
            File.Copy(Application.dataPath + "/Platform/Android/Plugins/AndroidLocalNotification.jar", Application.dataPath + "/Plugins/Android/AndroidLocalNotification.jar");
        if (!File.Exists(Application.dataPath + "/Plugins/Android/AndroidPermission.jar"))
            File.Copy(Application.dataPath + "/Platform/Android/Plugins/AndroidPermission.jar", Application.dataPath + "/Plugins/Android/AndroidPermission.jar");
        if (!File.Exists(Application.dataPath + "/Plugins/Android/WebViewPlugin.jar"))
            File.Copy(Application.dataPath + "/Platform/Android/Plugins/WebViewPlugin.jar", Application.dataPath + "/Plugins/Android/WebViewPlugin.jar");
        */


        /*

        // Resources Setting
        string[] resPath = { Application.dataPath + "/Plugins/Android/res/values/strings.xml" ,
                             Application.dataPath + "/Plugins/Android/res/values-en/strings.xml" ,
                             Application.dataPath + "/Plugins/Android/res/values-ko/strings.xml" ,
                             Application.dataPath + "/Plugins/Android/res/values-ja/strings.xml"
                            };

        for(int i=0; i<resPath.Length; i++)
        {
            XmlWriter Xmlwriter = XmlWriter.Create(resPath[i]);

            Xmlwriter.WriteStartDocument();
            Xmlwriter.WriteWhitespace("\r\n");

            Xmlwriter.WriteStartElement("resources");
            Xmlwriter.WriteWhitespace("\r\n");

            Xmlwriter.WriteStartElement("string");

            Xmlwriter.WriteAttributeString("name", "app_name");

            if (server == ServerType.Dev)
            {
                if (resPath[i].Contains("-ko"))
                    Xmlwriter.WriteString(PlayerSettings.productName + " Dev");
                else if (resPath[i].Contains("-ja"))
                    Xmlwriter.WriteString(PlayerSettings.productName + " Dev");
                else
                    Xmlwriter.WriteString(PlayerSettings.productName + " Dev");
            }
            else if (server == ServerType.Alpha)
            {
                if (resPath[i].Contains("-ko"))
                    Xmlwriter.WriteString(PlayerSettings.productName + " Alpha");
                else if (resPath[i].Contains("-ja"))
                    Xmlwriter.WriteString(PlayerSettings.productName + " Alpha");
                else
                    Xmlwriter.WriteString(PlayerSettings.productName + " Alpha");
            }
            else if (server == ServerType.Beta)
            {
                if (resPath[i].Contains("-ko"))
                    Xmlwriter.WriteString(PlayerSettings.productName + " Beta");
                else if (resPath[i].Contains("-ja"))
                    Xmlwriter.WriteString(PlayerSettings.productName + " Beta");
                else
                    Xmlwriter.WriteString(PlayerSettings.productName + " Beta");
            }

            Xmlwriter.WriteEndElement();
            Xmlwriter.WriteWhitespace("\r\n");

            Xmlwriter.WriteEndElement();
            Xmlwriter.WriteEndDocument();
            Xmlwriter.Close();
        }
        */
    }

    void ModifyProjectSetting(ServerType server, Publisher platformtype, bool IsSetIcon = false)
    {
        string IconPath = "";

        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            if (server == ServerType.Dev)
            {
                PlayerSettings.productName = m_BuildSetting.DevServer.ProductName;
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, m_BuildSetting.DevServer.BundleName);

                // Use log
                //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, string.Format("{0};{1}", "NO_PLUGIN", "ENABLE_LOG"));
            }
            else if (server == ServerType.Alpha)
            {
                PlayerSettings.productName = m_BuildSetting.AlphaServer.ProductName;
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, m_BuildSetting.AlphaServer.BundleName);
            }
            else if (server == ServerType.Beta)
            {
                PlayerSettings.productName = m_BuildSetting.BetaServer.ProductName;
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, m_BuildSetting.BetaServer.BundleName);
            }
            else if (server == ServerType.Review)
            {
                PlayerSettings.productName = m_BuildSetting.ReviewServer.ProductName;
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, m_BuildSetting.ReviewServer.BundleName);
            }
            else if (server == ServerType.Live)
            {
                PlayerSettings.productName = m_BuildSetting.LiveServer.ProductName;
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, m_BuildSetting.LiveServer.BundleName);
            }

            if (platformtype == Publisher.Self)
            {
                // nchant keystore 
                PlayerSettings.Android.keystoreName = m_BuildSetting.NchantAndroidKeyStoreName;
                PlayerSettings.Android.keystorePass = m_BuildSetting.NchantAndroidKeyStorePassword;
                PlayerSettings.Android.keyaliasName = m_BuildSetting.NchantAndroidKeyAlias;
                PlayerSettings.Android.keyaliasPass = m_BuildSetting.NchantAndroidKeyPassword;

                // icon
                IconPath = Application.dataPath + "/Icon/AOS_512x512_nchant.png";
            }

            // icon setting
            if (IsSetIcon)
            {
                int[] iconSize = new int[6] { 192, 144, 96, 72, 48, 36 };
                Texture2D[] icons = new Texture2D[6];

                for (int i = 0; i < icons.Length; i++)
                {
                    Texture2D icon = new Texture2D(iconSize[i], iconSize[i]);
                    icon.LoadImage(File.ReadAllBytes(IconPath));

                    icons[i] = icon;
                }
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, icons);
            }
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
        {           
            if (server == ServerType.Dev)
            {
                PlayerSettings.productName = m_BuildSetting.DevServer.ProductName;
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, m_BuildSetting.DevServer.BundleName);
            }
            else if (server == ServerType.Alpha)
            {
                PlayerSettings.productName = m_BuildSetting.AlphaServer.ProductName;
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, m_BuildSetting.AlphaServer.BundleName);
            }
            else if (server == ServerType.Beta)
            {
                PlayerSettings.productName = m_BuildSetting.BetaServer.ProductName;
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, m_BuildSetting.BetaServer.BundleName);
            }
            else if (server == ServerType.Review)
            {
                PlayerSettings.productName = m_BuildSetting.ReviewServer.ProductName;
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, m_BuildSetting.ReviewServer.BundleName);              
            }
            else if (server == ServerType.Live)
            {
                PlayerSettings.productName = m_BuildSetting.LiveServer.ProductName;
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, m_BuildSetting.LiveServer.BundleName);

            }

            // icon setting
            if (platformtype == Publisher.Self)
            {
                // icon
                IconPath = Application.dataPath + "/Icon/IOS_512x512_nchant.png";
            }

            if (IsSetIcon)
            { 
                int[] iconSize = new int[19] { 180, 167, 152, 144, 120, 114, 76, 72, 57, 120, 80, 40, 87, 58, 29, 60, 40, 20, 1024 };
                Texture2D[] icons = new Texture2D[19];

                for (int i = 0; i < icons.Length; i++)
                {
                    Texture2D icon = new Texture2D(iconSize[i], iconSize[i]);
                    icon.LoadImage(File.ReadAllBytes(IconPath));

                    icons[i] = icon;
                }
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.iOS, icons);
            }
        }
    }

    void Build(ServerType server, Publisher platformtype, string buildPath)
    {
         var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

        // Build player.
        if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            string archiveName = PlayerSettings.productName + "_" + server.ToString().ToLower() + "_" + platformtype.ToString().ToLower() + "_"
                                + PlayerSettings.bundleVersion + "_" + PlayerSettings.Android.bundleVersionCode + "_" + DateTime.Now.ToString("yyyMMddHHmm") + ".apk";

            BuildPipeline.BuildPlayer(scenes, buildPath + "/" + archiveName, BuildTarget.Android, BuildOptions.None);

            System.Diagnostics.Process.Start(buildPath);
        }
        else if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS,ScriptingImplementation.IL2CPP);

            BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.iOS, BuildOptions.None);
        }
    }
}
#endif

