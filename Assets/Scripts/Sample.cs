using Newtonsoft.Json;
using PatchSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Sample : MonoBehaviour {

    PatchManager PatchManager;

    // Use this for initialization
    void Start () {
        //ConfigManager.Load();
        //var name = DataManagerNavigator.Instance.GetManager<CdnEventChapterManager>().GetInfoByIndex(1).assetName;
        //Debug.Log(name);

        //PatchManager = new PatchManager(ConfigManager.PatchUrl);
        //PatchManager.Start();
        //StartCoroutine(Watching());

        
    }

    private void OnApplicationQuit()
    {
        //PatchManager.Stop();
    }


    IEnumerator Watching()
    {
        while(true)
        {
            Debug.Log(PatchManager.Info.DownloadFileCount + "/" + PatchManager.Info.TotalFileCount);
            Debug.Log(PatchManager.Info.DownloadFileSize + "/" + PatchManager.Info.TotalFileSize);

            if (PatchManager.Info.Error)
            {
                DownloadError();
            }
            else
            {
                if (PatchManager.Info.DownloadFileCount >= PatchManager.Info.TotalFileCount)
                {
                    Debug.Log("Download Complete");
                    RunGame();
                    break;
                }
            }

            yield return new WaitForSeconds(1);
            
        }
    }

    void DownloadError()
    {
        Debug.Log("DownloadError : " + PatchManager.Info.ErrorMessage);
        //StartCoroutine(Retry());
    }

    IEnumerator Retry()
    {
        Debug.Log("Error Download");
        yield return new WaitForSeconds(10f);
        PatchManager.Start();
    }

    void RunGame()
    {
        //패치로 받은 컨피그를 다시 읽는다.
        ConfigManager.LoadExternal();

        Debug.Log(ConfigManager.ServerUrl);

        //DataManagerNavigator.Instance.LoadInfo();

        //Debug.Log(DataManagerNavigator.Instance.g_LastModifiedUser);

        var temp = JsonConvert.DeserializeObject<string>("aa");

        //var name = DataManagerNavigator.Instance.GetManager<CdnMissionManager>().GetInfoByIndex(0).index;
        Debug.Log(name);

        
    }
   
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }


    }
}
