using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEngine;
using System.Reflection;
using System.Net;

namespace scavenger
{
    public static class Utils2
    {
        #region HttpWebRequest.AddRange(long)
        static MethodInfo httpWebRequestAddRangeHelper = typeof(WebHeaderCollection).GetMethod
                                                ("AddWithoutValidate", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// Adds a byte range header to a request for a specific range from the beginning or end of the requested data.
        /// </summary>
        /// <param name="request">The <see cref="System.Web.HttpWebRequest"/> to add the range specifier to.</param>
        /// <param name="start">The starting or ending point of the range.</param>
        public static void AddRange(this HttpWebRequest request, long start) { request.AddRange(start, -1L); }

        /// <summary>Adds a byte range header to the request for a specified range.</summary>
        /// <param name="request">The <see cref="System.Web.HttpWebRequest"/> to add the range specifier to.</param>
        /// <param name="start">The position at which to start sending data.</param>
        /// <param name="end">The position at which to stop sending data.</param>
        public static void AddRange(this HttpWebRequest request, long start, long end)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (start < 0) throw new ArgumentOutOfRangeException("start", "Starting byte cannot be less than 0.");
            if (end < start) end = -1;

            string key = "Range";
            string val = string.Format("bytes={0}-{1}", start, end == -1 ? "" : end.ToString());

            httpWebRequestAddRangeHelper.Invoke(request.Headers, new object[] { key, val });
        }
        #endregion
    }

    public class Utils
    {
        public static string GetHash(string filename,CryptoAlgoEnum cryptoAlgo)
        {
            HashAlgorithm algo = null;
            StringBuilder sb = new StringBuilder();

            switch (cryptoAlgo)
            { 
                case CryptoAlgoEnum.MD5:
                    algo = new MD5CryptoServiceProvider();
                    break;
                case CryptoAlgoEnum.SHA256:
                    algo = new SHA256Managed();// SHA256CryptoServiceProvider();
                    break;
                case CryptoAlgoEnum.SHA512:
                    algo = new SHA512Managed();// SHA512CryptoServiceProvider();
                    break;
            }


            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read)) 
            {
                foreach (Byte b in algo.ComputeHash(fs))
                    sb.Append(b.ToString("x2").ToLower());
            }

            return sb.ToString();
        }

        public static void SerializeToBinary(object o,Stream fs)
        {
            BinaryFormatter binfmt = new BinaryFormatter();
            binfmt.Serialize(fs, o);
        }

        public static object DeserializeFromBinary(Stream fs)
        {
            BinaryFormatter binfmt = new BinaryFormatter();
            return binfmt.Deserialize(fs);
        }

        public static void WriteLog(string message)
        {
            WriteLog(message, true);
        }

        public static void WriteLog(string message, bool maintenance)
        {
            Debug.Log(message);
            //try
            //{
            //    System.Diagnostics.StackFrame f = new System.Diagnostics.StackFrame(2);
            //    string methodname = f.GetMethod().Name;
            //    string fname = AppDomain.CurrentDomain.BaseDirectory + "scavenger.log";
            //    StreamWriter sw = new StreamWriter(fname, true);
            //    //message=String.Format ("{0:dd-MMM-yyyy HH:mm:ss}",DateTime.Now) + message;
            //    sw.WriteLine(string.Format("{0:dd-MMM-yyyy HH:mm:ss}", DateTime.Now) + ": " + methodname + ": " + message);
            //    sw.Close();

            //    if (maintenance)
            //    {
            //        FileInfo fi = new FileInfo(fname);
            //        if (fi.Length > 1000000000)
            //        {
            //            File.Delete(fname); //TODO: Clear initial n lines instead of deleting the entire file
            //        }
            //        fi = null;
            //    }
            //}
            //catch
            //{
            //}
        }
    }
}
