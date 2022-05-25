using scavenger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

namespace PatchSystem
{
    public class PatchManager
    {
        

        public class DownloadInfo
        {
            public int TotalFileCount;
            public int DownloadFileCount;
            public long TotalFileSize;
            public long DownloadFileSize;
            public bool Error = false;
            public string ErrorMessage;

            public void Init()
            {
                Error = false;
                ErrorMessage = "";
                DownloadFileCount = -1;
                TotalFileCount = -1;
                TotalFileSize = -1;
                DownloadFileSize = -1;
            }
        }

        public DownloadInfo Info = new DownloadInfo();

        Thread MainDownloadTread;

        List<Dictionary<string, string>> Files = new List<Dictionary<string, string>>();

        List<HttpDownload> HttpDownList = new List<HttpDownload>();

        string PatchURL;
        string DownloadPath;
        int ProcessorCount;

        string HashSaveFile;

        public PatchManager(string _ResourcePath)
        {
#if UNITY_EDITOR
            var savePath = Application.dataPath;
#else
            var savePath = Application.persistentDataPath;
#endif
            HashSaveFile = savePath + "/savehash.xml";

            PatchURL = _ResourcePath;


            var version = Application.version;

            var split = version.Split('.');

            if(split.Length >= 3)
            {
                var index = version.LastIndexOf(".");
                version = version.Remove(index, version.Length - index);
            }

            PatchURL += version + "/";

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                PatchURL += "ios/";
            }
            else
            {
                PatchURL += "aos/";
            }

            DownloadPath = Application.persistentDataPath + "/resources";
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            DownloadPath = Utility.RemoveLastPath(Application.dataPath) + "/resources";
#endif

            CreateFolder(DownloadPath);
#if UNITY_EDITOR
            Debug.Log(DownloadPath);
#endif
        }

        private void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        

        public void Start()
        {

            
            Stop();
            HttpDownList.Clear();

            Info.Init();
            ProcessorCount = SystemInfo.processorCount;
            var saveFile = DownloadPath + "/resource.xml";
            var patchFile = PatchURL + "resource.xml";

            WebClient client = new WebClient();
            client.DownloadFileCompleted += new AsyncCompletedEventHandler((sender, e) => client_DownloadFileCompleted(sender, e, new Uri(patchFile), saveFile));
            client.DownloadFileAsync(new Uri(patchFile), saveFile);
            
            
        }

        public void Stop()
        {
            for(int i =0;i< HttpDownList.Count; i++)
            {
                HttpDownList[i].Stop();
            }

            if(MainDownloadTread != null)
                MainDownloadTread.Abort();
        }

        void DownloadTreadRun(string _filename)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(_filename);
            var xnList = xml.SelectNodes("Info/Files/Item");
            foreach (XmlNode xn in xnList)
            {
                var dic = new Dictionary<string, string>();
                Files.Add(dic);
                for (int i = 0; i < xn.Attributes.Count; i++)
                {
                    dic.Add(xn.Attributes[i].Name, xn.Attributes[i].Value);
                }

                Info.TotalFileSize += Convert.ToInt64(dic["Size"]);
            }

            Info.TotalFileCount = Files.Count;
            Info.DownloadFileCount = 0;
            Info.DownloadFileSize = 0;
            

            
            for (int i = 0; i < Files.Count; i++)
            {
                string address = PatchURL + Files[i]["Path"];
                string filename = DownloadPath + "/" + Files[i]["Path"];

                
                var path = Utility.RemoveLastPath(Files[i]["Path"]);
                CreateFolder(DownloadPath + "/" + path);

                var needStatus = NeedCheck(filename, Files[i]["MD5"], Files[i]["Size"]);
                if (Files[i]["Path"].Contains("config.xml"))
                {
                    needStatus = DownloadNeedStatus.ReDownload;
                }

                if (needStatus == DownloadNeedStatus.Skip)
                {

                    Info.DownloadFileCount++;
                    Info.DownloadFileSize += Convert.ToInt64(Files[i]["Size"]);
                }
                else
                {
                    if(needStatus == DownloadNeedStatus.ReDownload)
                    {
                        File.Delete(filename);
                    }

                    HttpDownload httpDown = new HttpDownload(new Uri(address), filename);
                    HttpDownList.Add(httpDown);
                    
                }
            }

            long downloadedSize = Info.DownloadFileSize;
            int downloadedCount = Info.DownloadFileCount;
            int retryCount = 0;

            while (true)
            {
                
                int runCount = 0;
                int beforeDownloadCount = Info.DownloadFileCount;
                Info.DownloadFileCount = downloadedCount;
                Info.DownloadFileSize = downloadedSize;
                for (int i = 0; i < HttpDownList.Count; i++)
                {
                    if (HttpDownList[i].Status == DownloadStatusEnum.Running)
                    {
                        Info.DownloadFileSize += HttpDownList[i].BytesRead;
                        runCount++;
                    }
                    else if (HttpDownList[i].Status == DownloadStatusEnum.Completed)
                    {
                        Info.DownloadFileSize += HttpDownList[i].BytesRead;
                        Info.DownloadFileCount++;
                    }
                    
                }


                if(Info.DownloadFileCount >= Info.TotalFileCount)
                {
                    
                    CreateHashSave();
                    break;
                }

                
                if (runCount < ProcessorCount)
                {
                    for (int i = 0; i < HttpDownList.Count; i++)
                    {
                        if(HttpDownList[i].Status == DownloadStatusEnum.Prepared)
                        {
                            HttpDownList[i].Start();
                            runCount++;
                            break;
                        }
                        else if (HttpDownList[i].Status == DownloadStatusEnum.Error)
                        {
                            if (retryCount < 3)
                            {
                                HttpDownList[i].Start();
                                runCount++;
                                retryCount++;
                                break;
                            }
                            else
                            {
                                Info.Error = true;
                                Info.ErrorMessage = "Download Failed";
                                return;
                            }
                        }
                    }
                }

                Thread.Sleep(100);
                    
            }

            
        }


        void client_DownloadFileCompleted(object _sender, AsyncCompletedEventArgs _e, Uri _address, string _filename)
        {
            

            if (_e.Error != null)
            {
                Debug.LogError(_address.ToString() + "/" + _e.Error.Message);
                Info.Error = true;
                Info.ErrorMessage = _e.Error.Message;
                return;
            }

            GetHashSave();
            MainDownloadTread = new Thread(() => DownloadTreadRun(_filename));
            MainDownloadTread.Start();


        }

        Dictionary<string, string> HashCheckList = new Dictionary<string, string>();

        enum DownloadNeedStatus
        {
            Skip = 0,
            ReDownload,
            ResumeDownload
        }

        DownloadNeedStatus NeedCheck(string _targetFile, string _MD5, string _Size)
        {

            var fileInfo = new FileInfo(_targetFile);
            if (fileInfo.Exists)
            {
                if (fileInfo.Length == Convert.ToInt64(_Size))
                {
                    string hash = "";
                    if (HashCheckList.ContainsKey(_targetFile))
                    {
                        hash = HashCheckList[_targetFile];

                        if (hash == _MD5)
                        {
                            return DownloadNeedStatus.Skip;
                        }
                        else
                        {
                            return DownloadNeedStatus.ReDownload;
                        }
                    }
                    else
                    {
                        hash = Utility.CalculateMD5(_targetFile);
                        HashCheckList.Add(_targetFile, _MD5);

                        if (hash == _MD5)
                        {
                            return DownloadNeedStatus.Skip;
                        }
                        else
                        {
                            return DownloadNeedStatus.ResumeDownload;
                        }
                    }
                }
                else if(fileInfo.Length == 0)
                {
                    return DownloadNeedStatus.ReDownload;
                }
                else
                {
                    if (HashCheckList.ContainsKey(_targetFile) == false)
                    {
                        return DownloadNeedStatus.ResumeDownload;
                    }
                }
            }

            return DownloadNeedStatus.ReDownload;

        }

        void GetHashSave()
        {
            if (File.Exists(HashSaveFile) == false)
            {
                return;
            }

            XmlDocument xml = new XmlDocument();
            xml.Load(HashSaveFile);
            var xnList = xml.SelectNodes("Info/Files/Item");
            foreach (XmlNode xn in xnList)
            {
                HashCheckList.Add(xn.Attributes[0].Value, xn.Attributes[1].Value);
            }
        }

        void CreateHashSave()
        {
            if (HashCheckList.Count == 0)
                return;

            if ( File.Exists(HashSaveFile))
            {
                File.Delete(HashSaveFile);
            }

            var xDoc = new XDocument();
            var root = new XElement("Info");
            var files = new XElement("Files");

            foreach (var iter in HashCheckList)
            {
                files.Add(new XElement("Item",
                    new XAttribute("Path", iter.Key),
                    new XAttribute("MD5", iter.Value)
                    )
                    );
            }

            root.Add(files);
            xDoc.Add(root);

            xDoc.Save(HashSaveFile);

        }


        
    }

}