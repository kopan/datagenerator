using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace PatchSystem
{
    public class Utility : MonoBehaviour
    {

        public static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static string RemoveLastPath(string _Path)
        {
            var lastIndex = _Path.LastIndexOf("\\");
            if (lastIndex < 0)
            {
                lastIndex = _Path.LastIndexOf("/");
            }
            if (lastIndex >= 0)
            {
                return _Path.Remove(lastIndex, _Path.Length - lastIndex);
            }

            return _Path;
        }

        public static bool TryCast<T>(object obj, out T result)
        {
            if (obj is T)
            {
                result = (T)obj;
                return true;
            }

            result = default(T);
            return false;
        }
    }
}
