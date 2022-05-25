using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.S3;
using Amazon.S3.Model;
using UnityEngine;

public class UploadToS3 : MonoBehaviour
{
    private int uploadCount = 0;

    private int fileCount;
    // Start is called before the first frame update
    void Start()
    {
        return;

        UnityInitializer.AttachToGameObject(this.gameObject);
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;

        CognitoAWSCredentials credentials = new CognitoAWSCredentials(
            "ap-northeast-2:2d52f357-7172-4d2e-b47a-32e609811e89", // 자격 증명 풀 ID
            RegionEndpoint.APNortheast2 // 리전
        );

        AmazonS3Client s3Client = new AmazonS3Client (credentials, RegionEndpoint.APNortheast2);

        var path = Application.dataPath;
        path = path.Replace("/Assets", "");
        path = path + "/DataGeneratorForServer/xmlDataForServer/";

        uploadCount = 0;

        var info = new DirectoryInfo(path);
        var fileInfo = info.GetFiles();
        var uploadPath = "data/dev/xml/";
        var bucketName = "town-dev";
        var infoFileName = "info.xml";

        fileCount = fileInfo.Length - 1;

        Debug.Log("File Count : " + fileCount);

        foreach (var file in fileInfo)
        {
            if (file.Name.Contains(infoFileName))
            {
                continue;
            }

            var path2 = path + file.Name;

            var stream = new FileStream(path2, FileMode.Open, FileAccess.Read, FileShare.Read);

        
            var request = new PutObjectRequest()
            {
                BucketName = bucketName,
                Key = uploadPath + file.Name,
                InputStream = stream,
                CannedACL = S3CannedACL.Private,
            };

            Upload(s3Client, request);
        }

        var infoFilePath = path + infoFileName;
        var infoStream = new FileStream(infoFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        var infoRequest = new PutObjectRequest()
        {
            BucketName = bucketName,
            Key = uploadPath + "info.xml",
            InputStream = infoStream,
            CannedACL = S3CannedACL.Private,
        };

        StartCoroutine(WaitUpload(s3Client, infoRequest));
    }

    IEnumerator WaitUpload(AmazonS3Client s3Client, PutObjectRequest infoRequest)
    {
        while (true)
        {
            if (uploadCount >= fileCount)
            {
                Upload(s3Client, infoRequest);
        
                Debug.Log("Finish");
                break;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    
    void Upload(AmazonS3Client s3Client, PutObjectRequest request)
    {
        s3Client.PutObjectAsync(request, (responseObj) =>
        {
            if (responseObj.Exception == null)
            {
                Debug.Log(responseObj.Request.Key);
                Interlocked.Increment(ref uploadCount);
            }
            else
            {
                Debug.Log(string.Format("\n receieved error {0}", responseObj.Response.HttpStatusCode.ToString()));
            }
        });
    }

}
