using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace PatchSystem
{
    public class PatchCreater : MonoBehaviour
    {
        static readonly string ResourceFileName = "resource.xml";
        static readonly string ResourceFolderName = "/Resources/";

#if UNITY_EDITOR
        [MenuItem("Tools/Create resource.xml")]
        static void PrefabBuild()
        {

            Create();
            EditorUtility.DisplayDialog("create", "Success!!!", "ok");

        }
#endif



        static void FileSearch(string _oriDir, string targetDir, XElement _xFiles)
        {
            foreach (string f in Directory.GetFiles(targetDir))
            {
                var fileInfo = new FileInfo(f);

                if (fileInfo.Name == ResourceFileName || fileInfo.Extension == ".meta")
                {
                    continue;
                }

                _xFiles.Add(new XElement("Item",
                    new XAttribute("Path", f.Replace(_oriDir, "").Replace('\\','/')),
                    new XAttribute("Size", fileInfo.Length),
                    new XAttribute("MD5", Utility.CalculateMD5(f)),
                    new XAttribute("Date", fileInfo.LastWriteTime.ToString("yyyyMMddHHmmss"))
                    )
                    );

            }
        }

        static void DirSearch(string _oriDir, string targetDir, XElement _xFiles)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(targetDir))
                {
                    FileSearch(_oriDir, d, _xFiles);
                    DirSearch(_oriDir, d, _xFiles);
                }
            }
            catch (System.Exception excpt)
            {
                Debug.Log(excpt.Message);
            }
        }


        static public void Create()
        {
            ConfigManager.Load();
            var xDoc = new XDocument();
            var root = new XElement("Info",
                 new XAttribute("Device", ConfigManager.Platform),
                new XAttribute("Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                new XAttribute("Version", Application.version)
                );
            var files = new XElement("Files");
            string oriDir = Utility.RemoveLastPath(Application.dataPath) + ResourceFolderName;
            //FileSearch(oriDir, textBox1.Text, files);
            DirSearch(oriDir, oriDir, files);

            root.Add(files);
            xDoc.Add(root);
            var savePath = Utility.RemoveLastPath(Application.dataPath) + ResourceFolderName;
            xDoc.Save(savePath + ResourceFileName);
        }
    }

}