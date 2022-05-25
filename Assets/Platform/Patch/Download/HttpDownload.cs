using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace scavenger
{
    [Serializable]
    public class HttpDownload : Download
    {
        public HttpDownload(Uri url, string localPath) : base(url, localPath) { }
    }
}
