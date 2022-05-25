

using Debug = System.Diagnostics.Debug;
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

using System.IO;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class DataGeneratorWindow : EditorWindow
{
    static DataGeneratorWindow window = null;

    DataGenerator m_DataGenerator;

    // progress
    static public float m_fProgress = 0f;
    static public float m_fTotalProgress = 0f;
    static public string m_szProgressDesc ="";
    static public string m_szTotalProgressDesc = "";
    static public string m_szProgressStatus = "Ready!";
	static public string m_szDataNamePrefix = "";
    static public bool force_generate_all = false;

    static public ConcurrentStack<string> m_szTotalFileList = new ConcurrentStack<string>();
    static public ConcurrentStack<string> m_szTotalFileIDList = new ConcurrentStack<string>();

    [MenuItem("Tools/DataGenerator")]
    static public void Init()
    {
        // Get existing open window or if none, make a new one:
        window = (DataGeneratorWindow)EditorWindow.GetWindow(typeof(DataGeneratorWindow), false, "DataGenerator", true);
    }

    void OnEnable()
    {
        m_DataGenerator = new DataGenerator();

        m_DataGenerator.m_szGoogleDriveFolderName = PlayerPrefs.GetString("DataGenerator_GoogleDriveFolderName", "Sample");
		m_DataGenerator.m_szGoogleDriveFileNamePrefix = PlayerPrefs.GetString("DataGenerator_GoogleDriveFileName_Prefix", "");

        m_DataGenerator.m_szClientPath = PlayerPrefs.GetString("DataGenerator_ClientPath", "");
        m_DataGenerator.m_szServerPath = PlayerPrefs.GetString("DataGenerator_ServerPath", "");

    }

    
    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();
       
        EditorGUILayout.LabelField("[ Google Authorize & Drive Info ]");
        //EditorGUILayout.LabelField("Client ID", m_DataGenerator.m_szGoolgeClientID);
        //EditorGUILayout.LabelField("Client Sceret", m_DataGenerator.m_szGoogleClientSecret);
        string FolderName = EditorGUILayout.TextField("Drive Folder Name", m_DataGenerator.m_szGoogleDriveFolderName);

        if (m_DataGenerator.m_szGoogleDriveFolderName != FolderName)
        {
            m_DataGenerator.m_szGoogleDriveFolderName = FolderName;
            PlayerPrefs.SetString("DataGenerator_GoogleDriveFolderName",m_DataGenerator.m_szGoogleDriveFolderName);
        }

		string FilePrefix = EditorGUILayout.TextField("Drive File Name Prefix", m_DataGenerator.m_szGoogleDriveFileNamePrefix);

		if (m_DataGenerator.m_szGoogleDriveFileNamePrefix != FilePrefix)
		{
			m_DataGenerator.m_szGoogleDriveFileNamePrefix = FilePrefix;
			PlayerPrefs.SetString("DataGenerator_GoogleDriveFileName_Prefix", m_DataGenerator.m_szGoogleDriveFileNamePrefix);
		}

        string ClientPath = EditorGUILayout.TextField("ClientPath", m_DataGenerator.m_szClientPath);

        if (m_DataGenerator.m_szClientPath != ClientPath)
        {
            m_DataGenerator.m_szClientPath = ClientPath;
            PlayerPrefs.SetString("DataGenerator_ClientPath", m_DataGenerator.m_szClientPath);
        }

        string ServerPath = EditorGUILayout.TextField("ServerPath", m_DataGenerator.m_szServerPath);

        if (m_DataGenerator.m_szServerPath != ServerPath)
        {
            m_DataGenerator.m_szServerPath = ServerPath;
            PlayerPrefs.SetString("DataGenerator_ServerPath", m_DataGenerator.m_szServerPath);
        }


        EditorGUILayout.Space();

        force_generate_all = EditorGUILayout.Toggle( "Force ReGenerate All", force_generate_all);
        if (GUILayout.Button("Generate",GUILayout.Height(30)) == true)
        {
            if(EditorApplication.isCompiling)
            {
                UnityEngine.Debug.LogError("Cant Generate When Compiling!!");
                return;
            }

            m_fProgress = 0f;
            m_fTotalProgress = 0f;
            m_szProgressDesc = "";
            m_szTotalProgressDesc = "";
            m_szProgressStatus = "Ready!";

            m_szTotalFileList.Clear();
            m_szTotalFileIDList.Clear();

            m_DataGenerator.Excute();

            string mainPath = Application.dataPath.Replace("Assets", "");
            mainPath = mainPath.Replace("/", "\\");

#if UNITY_EDITOR_WIN
            if (ClientPath != null && ClientPath.Length > 0)
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = Path.GetFileName("copy_client.bat");
                psi.WorkingDirectory = Path.GetDirectoryName(mainPath);
                psi.Arguments = ClientPath;
                Process.Start(psi);
            }
            if (ServerPath != null && ServerPath.Length > 0)
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = Path.GetFileName("copy_server.bat");
                psi.WorkingDirectory = Path.GetDirectoryName(mainPath);
                psi.Arguments = ServerPath;
                Process.Start(psi);
            }
#else
            // mac 용 작업이 필요함

#endif
        }

        if (GUILayout.Button("Copy Only", GUILayout.Height(30)) == true)
        {
           
            string mainPath = Application.dataPath.Replace("Assets", "");
            mainPath = mainPath.Replace("/", "\\");

#if UNITY_EDITOR_WIN
            if (ClientPath != null && ClientPath.Length > 0)
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = Path.GetFileName("copy_client.bat");
                psi.WorkingDirectory = Path.GetDirectoryName(mainPath);
                psi.Arguments = ClientPath;
                Process.Start(psi);
            }
            if (ServerPath != null && ServerPath.Length > 0)
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = Path.GetFileName("copy_server.bat");
                psi.WorkingDirectory = Path.GetDirectoryName(mainPath);
                psi.Arguments = ServerPath;
                Process.Start(psi);
            }
#else
            // mac 용 작업이 필요함

#endif
        }

        m_DataGenerator.copy_folder_name = EditorGUILayout.TextField("Copy Folder Name", m_DataGenerator.copy_folder_name);
        if (GUILayout.Button("Folder Copy", GUILayout.Height(30)) == true)
        {
            m_DataGenerator.CopyFolder();
        }


        EditorGUILayout.Space();
        EditorGUILayout.LabelField("[ Progress Status ]");
        EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), m_fProgress , m_szProgressDesc);
        EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), m_fTotalProgress, m_szTotalProgressDesc);
        EditorGUILayout.LabelField("", m_szProgressStatus);
        EditorGUILayout.Space();

        if (m_szTotalFileList.Count>0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("[ Generate DataFiles ]");

            foreach(var iter in m_szTotalFileList)
            //for (int i = 0; i < m_szTotalFileList.Count; i++)
            {
                var style = GUI.skin.label;
                style.richText = true;
                string text = iter;
                text = string.Format("<color=#164CCC>{0}</color>", text);
                if (GUILayout.Button(text, GUI.skin.label))
                    Application.OpenURL("https://docs.google.com/spreadsheets/d/" + iter + "/edit#gid=0");
            }
            EditorGUILayout.Space();
        }
    }
}
#endif