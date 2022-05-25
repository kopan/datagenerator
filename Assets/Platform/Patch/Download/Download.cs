using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace scavenger
{
    [Serializable]
	public class Download
	{
		//public event System.EventHandler OnProgress;
        public CryptoAlgoEnum CryptoAlgo = CryptoAlgoEnum.None;
        public string CryptoKey = "";
        public DownloadStatusEnum Status = DownloadStatusEnum.Preparing;
        public int Size = 0;
        public int SizeInKB = 0;
        public long BytesRead = 0;
        public string FullFileName = null;
        public string FileName = null;
        public Uri Url;
        public bool UseProxy = false;
        public string ProxyServer = "";
        public int ProxyPort = 0;
        public string ProxyUsername = "";
        public string ProxyPassword = "";
        
        //public DateTime Started= Convert.ToDateTime("16-august-1981");

        /// <summary>
        /// Download speed in kilobytes/sec
        /// </summary>
        public double Speed = 0;//in KBPS

		
		//private Uri location=null;
		//private int segments=1;
		[NonSerialized] private Stream ns=null;
		[NonSerialized] private Stream fs=null;
        private bool acceptRanges = false;
        [NonSerialized] private Thread thStart;
        [NonSerialized] private Thread thPrepare;
        private DateTime ScheduledTime = DateTime.MinValue; //time at which download is scheduled to start
        [NonSerialized] private Stopwatch sw = new Stopwatch();

        public void OnDeserialize()
        {
            thStart = new Thread(StartThread);
            sw = new Stopwatch();
            //fs = new FileStream(FullFileName, FileMode.Append, FileAccess.Write);
        }

        public bool IsRunning()
        {
            return (thStart.ThreadState == System.Threading.ThreadState.Running);
        }

        private void CreateStreams(bool resuming)
        {
            //create file stream
            Utils.WriteLog("Creating local file stream.");

            if (!resuming)
            {
                fs = new FileStream(this.FullFileName, FileMode.Create, FileAccess.Write);
                fs.SetLength(Size);
            }
            else
            {
                //if (fs != null) fs.Close();
                fs = new FileStream(this.FullFileName, FileMode.Append, FileAccess.Write);
                BytesRead = fs.Position;
            }

            Utils.WriteLog("Created local file stream.");

            Utils.WriteLog("Creating network stream.");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            if (UseProxy)
            {
                request.Proxy = new WebProxy(ProxyServer + ":" + ProxyPort.ToString());
                if (ProxyUsername.Length>0) 
                    request.Proxy.Credentials = new NetworkCredential(ProxyUsername, ProxyPassword);
            }
            //HttpWebRequest hrequest = (HttpWebRequest)request;
            //hrequest.AddRange(BytesRead); ::TODO: Work on this
            if (BytesRead>0) request.AddRange(Convert.ToInt32(BytesRead));

            WebResponse response = request.GetResponse();
            //result.MimeType = res.ContentType;
            //result.LastModified = response.LastModified;
            if (!resuming)//(Size == 0)
            {
                //resuming = false;
                Size = (int)response.ContentLength;
                SizeInKB = (int)Size / 1024;
            }
            acceptRanges = String.Compare(response.Headers["Accept-Ranges"], "bytes", true) == 0;

            //create network stream
            ns = response.GetResponseStream();
            Utils.WriteLog("Created network stream.");
        }

        

		public Download(Uri url,string localfilename)
		{
            this.Url = url;
            this.FullFileName = localfilename;
            Status = DownloadStatusEnum.Preparing;

            thPrepare = new Thread( PrepareThread);
            thPrepare.Start();
		}

        public void Schedule(DateTime time)
        {
            ScheduledTime = time;
            Start();
        }

        public void Start()
        {
            Utils.WriteLog("Waiting for prepare thread to end.");
            if (thPrepare!=null && thPrepare.IsAlive) thPrepare.Join();
            Utils.WriteLog("Starting the main thread.");
            if (thStart != null && thStart.IsAlive) thStart.Abort();
            thStart = new Thread(StartThread);
            thStart.Start();
            Utils.WriteLog("Started the main thread.");
        }

        private void PrepareThread()
        {
            try
            {
                Status = DownloadStatusEnum.Preparing;
                Utils.WriteLog("Preparing download for url " + Url.OriginalString + ". localpath=" + FullFileName);

                //this.location = url;
                //this.segments=segments;

                //First, create the local file:
                FileInfo fi = new FileInfo(FullFileName);
                if (!Directory.Exists(fi.DirectoryName))
                    Directory.CreateDirectory(fi.DirectoryName);

                string fext = Path.GetExtension(FullFileName);
                bool resuming = false;
                if (File.Exists(FullFileName))
                {
                    resuming = true;
                    //int c = 0;
                    //string fname_woe = Path.GetFileNameWithoutExtension(FullFileName);
                    //string ffname = "";
                    //do
                    //{
                    //    ffname = fi.Directory.FullName + Path.DirectorySeparatorChar + fname_woe + string.Format("({0})", c++) + fext;
                    //} while (File.Exists(ffname));

                    //FullFileName = ffname;
                }

                this.FileName = Path.GetFileNameWithoutExtension(FullFileName) + fext;

                CreateStreams(resuming);

                Status = DownloadStatusEnum.Prepared;
                Utils.WriteLog("Prepared download.");
            }
            catch (Exception e)
            {
                Status = DownloadStatusEnum.Error;
                Utils.WriteLog("Error occured: " + e.Message);
            }        
        }

		private void StartThread()
		{
            try
            {
                Status = DownloadStatusEnum.Paused;
                while (ScheduledTime > DateTime.Now)
                { }


                this.Status = DownloadStatusEnum.Running;
                //TODO: Check for thread-safety and lock/sync variables accordingly.
                byte[] buffer = new byte[4096];
                int bytesToRead = Size;
                //if (Started.Year==1981) Started = DateTime.Now;
                sw.Start();
                while (true) //(bytesToRead>0)
                {
                    int n = ns.Read(buffer, 0, buffer.Length);
                    //sw.Stop();
                    //Speed = (n == 0) ? 0 : ((float)n / 1000) / sw.Elapsed.TotalSeconds;
                    Speed = (n == 0) ? 0 : ((float)BytesRead / 1000) / sw.Elapsed.TotalSeconds;

                    if (n == 0)
                    {
                        Status = DownloadStatusEnum.Completing;
                        break;
                    }
                    fs.Write(buffer, 0, n);
                    fs.Flush();
                    BytesRead += n;
                    bytesToRead -= n;
                    //OnProgress(this, new System.EventArgs());
                    if (Status == DownloadStatusEnum.Pausing || Status == DownloadStatusEnum.Stopping) break;
#if DEBUG
                    Thread.Sleep(10);
#endif
                }
                sw.Stop();

                ns.Close(); ns = null;
                fs.Close();

                if (Status == DownloadStatusEnum.Pausing) 
                    Status = DownloadStatusEnum.Paused;
                else if (Status == DownloadStatusEnum.Completing)
                {
                    //verify before completing
                    if (CryptoAlgo != CryptoAlgoEnum.None)
                    {
                        if (Utils.GetHash(FullFileName, CryptoAlgo) != this.CryptoKey)
                        { 
                            //TODO: Indicate in some manner that verfication has failed.
                        }
                    }
                    Status = DownloadStatusEnum.Completed;
                }
                else if (Status == DownloadStatusEnum.Stopping)
                {
                    File.Delete(FullFileName);
                    Status = DownloadStatusEnum.Stopped;
                }
                //OnProgress(this, new System.EventArgs());
            }
            catch (Exception)
            {
                Status = DownloadStatusEnum.Error;
                //OnProgress(this, new EventArgs());
            }
		}

        public void Resume()
        {
            //TODO: Test this code thoroughly.
            
            //ns.Position = BytesRead; No need as this will be handled by request.AddRange() in CreateNetworkStream()
            CreateStreams(true);
            Start();
        }

        public void Pause()
        {
            Status = DownloadStatusEnum.Pausing;
            if (thStart.IsAlive) thStart.Join();
        }

        public void Stop()
        {
            if (thStart == null || thStart.ThreadState == System.Threading.ThreadState.Unstarted
                || thStart.ThreadState == System.Threading.ThreadState.Stopped)
            {
                Status = DownloadStatusEnum.Stopped;
                return;
            }
            Status = DownloadStatusEnum.Stopping;

            thStart.Abort();
            thStart.Join(5 * 1000);

            Status = DownloadStatusEnum.Stopped;
        }
	}
}

