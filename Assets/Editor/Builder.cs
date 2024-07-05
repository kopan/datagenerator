using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using UnityEditor.Build.Reporting;
using UnityEditorInternal;
using Debug = System.Diagnostics.Debug;

public class Builder
{
    static Windows.ConsoleWindow console = new Windows.ConsoleWindow();

    public static void CopyFolder()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        console.Initialize();
        console.SetTitle("datagenerator");
        
        Application.logMessageReceived += HandleLog;

        UnityEngine.Debug.Log("Console Started");
#endif

        string[] args = Environment.GetCommandLineArgs();

        string ori_folder = null;
        string copy_folder = null;
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i - 1].Equals("-ori_folder")) ori_folder = args[i];
            if (args[i - 1].Equals("-copy_folder")) copy_folder = args[i];
        }

        var m_DataGenerator = new DataGenerator();
        m_DataGenerator.copy_folder_name = copy_folder;
        m_DataGenerator.m_szGoogleDriveFolderName = ori_folder;

        m_DataGenerator.CopyFolder();

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

        console.Shutdown();
#endif
    }

    [MenuItem("Tools/DataGenerator.Excute")]
	public static  void DataGenerator()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        console.Initialize();
        console.SetTitle( "datagenerator" );
 
        
 
        Application.logMessageReceived += HandleLog;

        UnityEngine.Debug.Log( "Console Started" );
#endif

        string path = Application.dataPath;
        path = path.Replace("/Assets", "");


        string[] args = Environment.GetCommandLineArgs();
        string type = "";
        string folder = "PokoTown";
        bool isForceGenAll = false;
        for (int i = 1; i < args.Length; i++)
        {
            //if (args[i - 1].Equals("-clear")) type = args[i];
            if (args[i - 1].Equals("-folder")) folder = args[i];
            if (args[i - 1].Equals("-forcegenall"))
            {
                string strForceGenAll = args[i];
                if (false == string.IsNullOrEmpty(strForceGenAll) &&
                    bool.TryParse(strForceGenAll, out isForceGenAll)) { }
            }
        }
        //bool clear = false;
        //if (Boolean.TryParse(type, out clear))
        //{
        //
        //}
        //if (clear)
        //{
        //    string pathUpdate = Path.Combine(Application.dataPath, "last_update_at.json");
        //    System.IO.FileInfo fi = new System.IO.FileInfo(pathUpdate);
        //    if (fi.Exists)
        //    {
        //        fi.Delete();
        //    }
        //
        //    DeleteDirectoryPath(string.Format("{0}/{1}", path, "DataGeneratorForServer/ServerDataManager_CSharp"));
        //    DeleteDirectoryPath(string.Format("{0}/{1}", path, "DataGeneratorForServer/Text"));
        //    DeleteDirectoryPath(string.Format("{0}/{1}", path, "DataGeneratorForServer/xmlDataForServer"));
        //    DeleteDirectoryPath(string.Format("{0}/{1}", path, "resources/Text"));
        //    DeleteDirectoryPath(string.Format("{0}/{1}", path, "resources/xeData"));
        //    DeleteDirectoryPath(string.Format("{0}/{1}", Application.dataPath, "DataScripts"));
        //
        //}
        string ClientPath = PlayerPrefs.GetString("DataGenerator_ClientPath", "");
        string mainPath = Application.dataPath.Replace("Assets", "");
       

        Generator(folder, isForceGenAll);

#if UNITY_EDITOR_WIN
        if (string.IsNullOrEmpty(ClientPath) == false)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = Path.GetFileName("copy_client.bat");
            psi.WorkingDirectory = Path.GetDirectoryName(mainPath);
            psi.Arguments = ClientPath;
            Process.Start(psi);
        }

        
#else
        

#endif

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

        console.Shutdown();
#endif

    }

    static void HandleLog( string message, string stackTrace, LogType type )
    {
        if ( type == LogType.Warning )		
            System.Console.ForegroundColor = ConsoleColor.Yellow;
        else if ( type == LogType.Error )	
            System.Console.ForegroundColor = ConsoleColor.Red;
        else								
            System.Console.ForegroundColor = ConsoleColor.White;

        // We're half way through typing something, so clear this line ..
        //if ( Console.CursorLeft != 0 )
        //    input.ClearLine();
 
        System.Console.WriteLine( message );
 
        // If we were typing something re-add it.
        //input.RedrawInputLine();
    }

    public static void DeleteDirectoryPath(string dir)
    {
        try
        {
            //DirectoryInfoオブジェクトの作成
            DirectoryInfo di = new DirectoryInfo(dir);

            //フォルダ以下のすべてのファイル、フォルダの属性を削除
            RemoveReadonlyAttribute(di);

            //フォルダを根こそぎ削除
            di.Delete(true);

     
        }
        catch(Exception e)
        {
            
        }
    }

    public static void RemoveReadonlyAttribute(DirectoryInfo dirInfo)
    {
        //基のフォルダの属性を変更
        if ((dirInfo.Attributes & FileAttributes.ReadOnly) ==
            FileAttributes.ReadOnly)
            dirInfo.Attributes = FileAttributes.Normal;
        //フォルダ内のすべてのファイルの属性を変更
        foreach (FileInfo fi in dirInfo.GetFiles())
            if ((fi.Attributes & FileAttributes.ReadOnly) ==
                FileAttributes.ReadOnly)
                fi.Attributes = FileAttributes.Normal;
        //サブフォルダの属性を回帰的に変更
        foreach (DirectoryInfo di in dirInfo.GetDirectories())
            RemoveReadonlyAttribute(di);
    }

    private static void Generator(string folder, bool isForceGenAll = false)
    {
        var m_DataGenerator = new DataGenerator();
        m_DataGenerator.m_szGoogleDriveFileNamePrefix = PlayerPrefs.GetString("DataGenerator_GoogleDriveFileName_Prefix", "");

        m_DataGenerator.m_szClientPath = PlayerPrefs.GetString("DataGenerator_ClientPath", "");
        m_DataGenerator.m_szServerPath = PlayerPrefs.GetString("DataGenerator_ServerPath", "");
        m_DataGenerator.m_szGoogleDriveFolderName = folder;

        DataGeneratorWindow.force_generate_all = isForceGenAll;

        m_DataGenerator.Excute();

        
        
    }
}
