using UnityEngine;
using UnityEditor;

using System;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Data;
using System.Threading;

using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using NUnit.Framework.Interfaces;
using UnityEditor.PackageManager;
using UnityEngine.Animations;
using Debug = UnityEngine.Debug;

public class DataGenerator
{
    // google spread sheet

    public string m_szGoogleDriveFolderName = "PokoTown";
	public string m_szGoogleDriveFileNamePrefix;

    public string m_szClientPath;
    public string m_szServerPath;

    public string m_dataPath;

    public string copy_folder_name;
    public string teamdrive_folder_id = "";

    [Serializable]
    public class LastUpdateInfo
    {
        public string Name;
        public string DateTime;
        public bool IsClient = false;
        public bool IsServer = false;       
    }

    [Serializable]
    public class LastUpdateInfoMain
    {
        public LastUpdateInfo[] infos;
    }
    
    public ConcurrentQueue<string> ErrorLog_Queue = new ConcurrentQueue<string>();
    public ConcurrentQueue<string> Log_Queue = new ConcurrentQueue<string>();
    ConcurrentDictionary<string, LastUpdateInfo> LastUpdateInfos = new ConcurrentDictionary<string, LastUpdateInfo>();

    void CreateDirectory(string szDirName)
    {
        if (!Directory.Exists(Application.dataPath + szDirName))
            Directory.CreateDirectory(Application.dataPath + szDirName);
    }

    bool SaveXmlFile(bool isClientXml, string FileName, DataTable definition, DataTable table, string update_at)
    {
        // create Xml stream
        MemoryStream xmlStream = new MemoryStream();

        XmlWriter Writer = XmlWriter.Create(xmlStream);
        Writer.WriteStartDocument();

        Writer.WriteWhitespace("\n");
        Writer.WriteStartElement("Data");

        for (int i = 0; i < table.Rows.Count; i++)
        {
            Writer.WriteWhitespace("\n");
            Writer.WriteStartElement("Data");

            for (int j = 0; j < table.Columns.Count && j < definition.Rows.Count; j++)
            {
                string szDependencyAttr = definition.Rows[j].ItemArray[3].ToString();
                if ((szDependencyAttr == "0") ||
                    (isClientXml && szDependencyAttr == "2") ||
                    (!isClientXml && szDependencyAttr == "1")) continue;

                string szAttribute = definition.Rows[j].ItemArray[2].ToString();
                string name = definition.Rows[j].ItemArray[0].ToString();
                string value = table.Rows[i].ItemArray[j].ToString();

                // check Data
                if (szAttribute.ToLower() == "byte" || szAttribute.ToLower() == "short" || szAttribute.ToLower() == "int" ||
                    szAttribute.ToLower() == "long" || szAttribute.ToLower() == "float")
                {
                    if (value == "" || value.Length == 0)
                    {
                        var error = "error :" + FileName + " Check Definition sheet (col: " + name + " row: " +
                                    i.ToString() + ")";
                        ErrorLog_Queue.Enqueue(error);
                        //Debug.LogError(error);
                        LastUpdateInfo info;
                        LastUpdateInfos.TryRemove(FileName, out info);
                        return false;
                    }
                }

                Writer.WriteAttributeString(name, value);
            }

            Writer.WriteEndElement();
        }

        Writer.WriteWhitespace("\n");
        Writer.WriteEndElement();

        Writer.WriteEndDocument();

        Writer.Flush();
        Writer.Close();

        // Write xml
        string szXmlFileName = isClientXml ?
                                m_dataPath + "/StreamingAssets/xmlData/" + FileName + ".xml" :
                                m_dataPath + "/../DataGeneratorForServer/xmlDataForServer/" + FileName + ".xml";

        FileStream outStream = new FileStream(szXmlFileName, FileMode.Create, FileAccess.Write);
        xmlStream.WriteTo(outStream);

        outStream.Flush();
        outStream.Close();

        if (isClientXml)
        {
            // Write encoding client xml for patch
            szXmlFileName = m_dataPath + "/../Resources/xeData/" + FileName + ".xml";

            FileStream outEncStream = new FileStream(szXmlFileName, FileMode.Create, FileAccess.Write);

            byte[] Key = new byte[32];
            byte[] IV = new byte[16];
            byte[] FileKey = Encoding.UTF8.GetBytes(Path.GetFileNameWithoutExtension(szXmlFileName).ToCharArray());
            Array.Clear(Key, 0, 32);
            Array.Copy(FileKey, Key, (FileKey.Length > 32) ? 32 : FileKey.Length);
            Array.Copy(Key, IV, 16);

            RijndaelManaged aes = new RijndaelManaged();
            ICryptoTransform cryptoTransform = aes.CreateEncryptor(Key, IV);
            CryptoStream cryptoStream = new CryptoStream(outEncStream, cryptoTransform, CryptoStreamMode.Write);
            cryptoStream.Write(xmlStream.ToArray(), 0, (int)xmlStream.Length);
            cryptoStream.FlushFinalBlock();

            outEncStream.Flush();
            outEncStream.Close();

            File.Copy(szXmlFileName, m_dataPath + "/StreamingAssets/xeData/" + FileName + ".xml", true);
        }

        xmlStream.Flush();
        xmlStream.Close();
        
        
        return true;
    }

    void WriteSource_CSharp( bool IsClientSource, string FileName, DataTable Dt, DataTable Data_Dt)
    {
        string szXmlPath4Client = "/StreamingAssets/xmlData/" + FileName + ".xml";
        string szXmlEncPath4Client = "/xeData/" + FileName + ".xml";

        string szXmlPath4Server = "DataGeneratorForServer/xmlDataForServer/" + FileName + ".xml";

        string szStructName = FileName;
		string szClassName = FileName + "Manager";

        string szAttributeType = null;
        string szAttribute = null;
        string szLoadText = null;

        bool isServerCode = false;
        bool isClientCode = false;

        if (System.IO.File.Exists(m_dataPath + "/Platform/Editor/DataGenerator/DataManagerFormat_CSharp.txt"))
        {
            string[] aReadLines = System.IO.File.ReadAllLines(m_dataPath + "/Platform/Editor/DataGenerator/DataManagerFormat_CSharp.txt");
            List<string> aLines = new List<string>();

            foreach (string szLine in aReadLines)
            {
                if (szLine.Contains("#SERVER_CODE_START#"))
                {
                    isServerCode = true;
                    continue;
                }

                if (szLine.Contains("#SERVER_CODE_END#"))
                {
                    isServerCode = false;
                    continue;
                }

                if (szLine.Contains("#CLIENT_CODE_START#"))
                {
                    isClientCode = true;
                    continue;
                }

                if (szLine.Contains("#CLIENT_CODE_END#"))
                {
                    isClientCode = false;
                    continue;
                }

                if (IsClientSource && isServerCode) continue;

                if (!IsClientSource && isClientCode) continue;


                if (szLine.Contains("#ENUMS#"))
                {
                    if (FileName.ToLower().Contains("btest") == false)
                    {
                        var type = Dt.Rows[0].ItemArray[2].ToString();
                        if (type.Contains("enum "))
                        {
                            aLines.Add(string.Format("public enum {0}", type.Replace("enum ", "")));
                            aLines.Add("{");
                            aLines.Add("\tNone = -1,");

                            int i = 0;
                            for (i = 0;
                                 i < Data_Dt.Rows.Count && Data_Dt.Rows[i].ItemArray[0].ToString().Length > 0;
                                 i++)
                            {
                                aLines.Add(string.Format("\t{0} = {1},", Data_Dt.Rows[i].ItemArray[0], i + 1));
                            }

                            aLines.Add(string.Format("\tEnd = {0}", i + 1));
                            aLines.Add("}");
                        }
                    }
                    else
                    {
                        aLines.Add("");
                    }
                }


                else if (szLine.Contains("#INFO_MEMBER#"))
                {
                    for (int i = 0; i < Dt.Rows.Count && Dt.Rows[i].ItemArray[0].ToString().Length > 0; i++)
                    {
                        szAttributeType = Dt.Rows[i].ItemArray[3].ToString();
                        if ((szAttributeType == "0") ||
                            (IsClientSource && szAttributeType == "2") ||
                            (!IsClientSource && szAttributeType == "1")) continue;

                        szAttribute = "\t/// <summary> " + Dt.Rows[i].ItemArray[1].ToString().Replace("\n", ",") + " </summary>";
                        aLines.Add(szAttribute);

                        szAttribute = Dt.Rows[i].ItemArray[2].ToString();
                        if (szAttribute.ToLower() == "byte") szAttribute = "\tpublic byte";
                        else if (szAttribute.ToLower() == "short") szAttribute = "\tpublic short";
                        else if (szAttribute.ToLower() == "int") szAttribute = "\tpublic int";
                        else if (szAttribute.ToLower() == "long") szAttribute = "\tpublic Int64";
                        else if (szAttribute.ToLower() == "float") szAttribute = "\tpublic float";
                        else if (szAttribute.ToLower() == "array_byte") szAttribute = "\tpublic byte[]";
                        else if (szAttribute.ToLower() == "array_int") szAttribute = "\tpublic int[]";
                        else if (szAttribute.ToLower() == "array_float") szAttribute = "\tpublic float[]";
                        else if (szAttribute.ToLower() == "array_string") szAttribute = "\tpublic string[]";
                        else if (szAttribute.ToLower() == "list_int") szAttribute = "\tpublic List<int>";
                        else if (szAttribute.ToLower() == "list_float") szAttribute = "\tpublic List<float>";
                        else if (szAttribute.ToLower() == "list_string") szAttribute = "\tpublic List<string>";
                        else if (szAttribute.ToLower() == "string") szAttribute = "\tpublic string";
                        else if (szAttribute.ToLower().Contains("enum"))
                        {
                            string[] text = szAttribute.Split(' ');
                            szAttribute = "\tpublic " + text[1].Trim();
                        }
                        else szAttribute = "\tpublic " + szAttribute;

                        szAttribute += "\t" + Dt.Rows[i].ItemArray[0].ToString() + ";";
                        aLines.Add(szAttribute);
                    }
                }

                else if (szLine.Contains("#SET_UP_XML_DATA#"))
                {
                    for (int i = (Dt.Rows.Count - 1); i >= 0 && Dt.Rows[i].ItemArray[0].ToString().Length > 0; i--)
                    {
                        szAttributeType = Dt.Rows[i].ItemArray[3].ToString();
                        if ((szAttributeType == "0") ||
                            (IsClientSource && szAttributeType == "2") ||
                            (!IsClientSource && szAttributeType == "1"))
                        {
                            szLoadText = "dt.Columns.RemoveAt(" + i.ToString() + ");";

                            aLines.Add(szLine.Replace("#SET_UP_XML_DATA#", szLoadText));
                        }
                    }
                }

                else if (szLine.Contains("#GET_INFO_MAPPING#"))
                {
                    int rowIndex = 0;
                    for (int i = 0; i < Dt.Rows.Count && Dt.Rows[i].ItemArray[0].ToString().Length > 0; i++)
                    {
                        szAttributeType = Dt.Rows[i].ItemArray[3].ToString();
                        if ((szAttributeType == "0") ||
                            (IsClientSource && szAttributeType == "2") ||
                            (!IsClientSource && szAttributeType == "1")) continue;

                        szLoadText = (rowIndex > 0) ? ", " : "";
                        szLoadText += Dt.Rows[i].ItemArray[0].ToString() + " = ";

                        szAttribute = Dt.Rows[i].ItemArray[2].ToString();
                        if (szAttribute.ToLower() == "byte")
                        {
                            szLoadText += " Convert.ToByte(row[" + rowIndex.ToString() + "].ToString())";
                        }
                        else if (szAttribute.ToLower() == "short")
                        {
                            szLoadText += " Convert.ToInt16(row[" + rowIndex.ToString() + "].ToString())";
                        }
                        else if (szAttribute.ToLower() == "int")
                        {
                            szLoadText += " Convert.ToInt32(row[" + rowIndex.ToString() + "].ToString())";
                        }
                        else if (szAttribute.ToLower() == "long")
                        {
                            szLoadText += " Convert.ToInt64(row[" + rowIndex.ToString() + "].ToString())";
                        }
                        else if (szAttribute.ToLower() == "float")
                        {
                            szLoadText += " Convert.ToSingle(row[" + rowIndex.ToString() + "].ToString())";
                        }
                        else if (szAttribute.ToLower() == "array_byte")
                        {
                            szLoadText += " String.IsNullOrEmpty(row[" + rowIndex.ToString() + "].ToString()) ? new byte[0] : Array.ConvertAll<string,byte>(row[" + rowIndex.ToString() + "].ToString().Split(','),byte.Parse)";
                        }
                        else if (szAttribute.ToLower() == "array_int")
                        {
                            szLoadText += " String.IsNullOrEmpty(row[" + rowIndex.ToString() + "].ToString()) ? new int[0] : Array.ConvertAll<string,int>(row[" + rowIndex.ToString() + "].ToString().Split(','),int.Parse)";
                        }
                        else if (szAttribute.ToLower() == "array_float")
                        {
                            szLoadText += " String.IsNullOrEmpty(row[" + rowIndex.ToString() + "].ToString()) ? new float[0] : Array.ConvertAll<string,float>(row[" + rowIndex.ToString() + "].ToString().Split(','),float.Parse)";
                        }
                        else if (szAttribute.ToLower() == "array_string")
                        {
                            szLoadText += " String.IsNullOrEmpty(row[" + rowIndex.ToString() + "].ToString()) ? new string[0] : row[" + rowIndex.ToString() + "].ToString().Split(',')";
                        }
                        else if (szAttribute.ToLower() == "list_int")
                        {
                            szLoadText += " String.IsNullOrEmpty(row[" + rowIndex.ToString() + "].ToString()) ? new List<int>() : Array.ConvertAll<string,int>(row[" + rowIndex.ToString() + "].ToString().Split(','),int.Parse).ToList()";
                        }
                        else if (szAttribute.ToLower() == "list_float")
                        {
                            szLoadText += " String.IsNullOrEmpty(row[" + rowIndex.ToString() + "].ToString()) ? new List<float>() : Array.ConvertAll<string,float>(row[" + rowIndex.ToString() + "].ToString().Split(','),float.Parse).ToList()";
                        }
                        else if (szAttribute.ToLower() == "list_string")
                        {
                            szLoadText += " String.IsNullOrEmpty(row[" + rowIndex.ToString() + "].ToString()) ? new List<string>() : new List<string>(row[" + rowIndex.ToString() + "].ToString().Split(','))";
                        }
                        else if (szAttribute.ToLower() == "string")
                        {
                            szLoadText += " row[" + rowIndex.ToString() + "].ToString()";
                        }
                        else if (szAttribute.ToLower().Contains("enum"))
                        {
                            string[] text = szAttribute.Split(' ');

                            szLoadText += " Enum.Parse<" + text[1] + ">(row[" + rowIndex.ToString() + "].ToString())";

                        }
                        else
                        {
                            var t = szAttribute;
                            szLoadText += string.Format(" Newtonsoft.Json.JsonConvert.DeserializeObject<{0}>(row[{1}].ToString()", t, rowIndex.ToString()) + ")";

                        }

                        aLines.Add(szLine.Replace("#GET_INFO_MAPPING#", szLoadText));
                        rowIndex++;
                    }
                }

                else if (szLine.Contains("#GET_INFO_MAPPING_BY_REFLECTION#"))
                {
                    int rowIndex = 0;
                    for (int i = 0; i < Dt.Rows.Count && Dt.Rows[i].ItemArray[0].ToString().Length > 0; i++)
                    {
                        szAttributeType = Dt.Rows[i].ItemArray[3].ToString();
                        if ((szAttributeType == "0") ||
                            (IsClientSource && szAttributeType == "2") ||
                            (!IsClientSource && szAttributeType == "1")) continue;

                        szLoadText = "if(info.GetType().GetField(\"";
                        szLoadText += Dt.Rows[i].ItemArray[0].ToString();
                        szLoadText += "\") != null) { if (row[" + rowIndex.ToString();
                        szLoadText += "].ToString().Length > 0) info.GetType().GetField(\"";
                        szLoadText += Dt.Rows[i].ItemArray[0].ToString();
                        szLoadText += "\").SetValue(info,";

                        szAttribute = Dt.Rows[i].ItemArray[2].ToString();
                        if (szAttribute.ToLower() == "byte")
                        {
                            szLoadText += "Convert.ToByte(row[" + rowIndex.ToString() + "].ToString())";
                        }
                        else if (szAttribute.ToLower() == "short")
                        {
                            szLoadText += "Convert.ToInt16(row[" + rowIndex.ToString() + "].ToString())";
                        }
                        else if (szAttribute.ToLower() == "int")
                        {
                            szLoadText += "Convert.ToInt32(row[" + rowIndex.ToString() + "].ToString())";
                        }
                        else if (szAttribute.ToLower() == "long")
                        {
                            szLoadText += "Convert.ToInt64(row[" + rowIndex.ToString() + "].ToString())";
                        }
                        else if (szAttribute.ToLower() == "float")
                        {
                            szLoadText += "Convert.ToSingle(row[" + rowIndex.ToString() + "].ToString())";
                        }
                        else if (szAttribute.ToLower() == "array_byte")
                        {
                            szLoadText += "String.IsNullOrEmpty(row[" + rowIndex.ToString() + "].ToString()) ? new byte[0] : Array.ConvertAll<string,byte>(row[" + rowIndex.ToString() + "].ToString().Split(','),byte.Parse)";
                        }
                        else if (szAttribute.ToLower() == "array_int")
                        {
                            szLoadText += "String.IsNullOrEmpty(row[" + rowIndex.ToString() + "].ToString()) ? new int[0] : Array.ConvertAll<string,int>(row[" + rowIndex.ToString() + "].ToString().Split(','),int.Parse)";
                        }
                        else if (szAttribute.ToLower() == "array_float")
                        {
                            szLoadText += "String.IsNullOrEmpty(row[" + rowIndex.ToString() + "].ToString()) ? new float[0] : Array.ConvertAll<string,float>(row[" + rowIndex.ToString() + "].ToString().Split(','),float.Parse)";
                        }
                        else if (szAttribute.ToLower() == "array_string")
                        {
                            szLoadText += "String.IsNullOrEmpty(row[" + rowIndex.ToString() + "].ToString()) ? new string[0] : row[" + rowIndex.ToString() + "].ToString().Split(',')";
                        }
                        else if (szAttribute.ToLower() == "list_int")
                        {
                            szLoadText += "String.IsNullOrEmpty(row[" + rowIndex.ToString() + "].ToString()) ? new List<int>() : Array.ConvertAll<string,int>(row[" + rowIndex.ToString() + "].ToString().Split(','),int.Parse).ToList()";
                        }
                        else if (szAttribute.ToLower() == "list_float")
                        {
                            szLoadText += "String.IsNullOrEmpty(row[" + rowIndex.ToString() + "].ToString()) ? new List<float>() : Array.ConvertAll<string,float>(row[" + rowIndex.ToString() + "].ToString().Split(','),float.Parse).ToList()";
                        }
                        else if (szAttribute.ToLower() == "list_string")
                        {
                            szLoadText += "String.IsNullOrEmpty(row[" + rowIndex.ToString() + "].ToString()) ? new List<string>() : new List<string>(row[" + rowIndex.ToString() + "].ToString().Split(','))";
                        }
                        else if (szAttribute.ToLower() == "string")
                        {
                            szLoadText += "row[" + rowIndex.ToString() + "].ToString()";
                        }
                        else if (szAttribute.ToLower().Contains("enum"))
                        {
                            string[] text = szAttribute.Split(' ');

                            szLoadText += " Convert.ToInt32((int)Enum.Parse(typeof(" + text[1] + "), row[" + rowIndex.ToString() + "].ToString()))";

                        }
                        else
                        {
                            szLoadText = "if(info.GetType().GetField(\"";
                            szLoadText += Dt.Rows[i].ItemArray[0].ToString();
                            szLoadText += "\") != null) { info.";
                            szLoadText += Dt.Rows[i].ItemArray[0].ToString();

                            var t = szAttribute;
                            szLoadText += string.Format(" = Newtonsoft.Json.JsonConvert.DeserializeObject<{0}>(row[{1}].ToString()", t, rowIndex.ToString());

                        }

                        szLoadText += ");} else UnityEngine.Debug.LogError(\"Exception : ";
                        szLoadText += szStructName + Dt.Rows[i].ItemArray[0].ToString() + "\");";

                        aLines.Add(szLine.Replace("#GET_INFO_MAPPING_BY_REFLECTION#", szLoadText));
                        rowIndex++;
                    }
                }
                else if (szLine.Contains("#GET_INFO_MAPPING_BY_REFLECTION2#"))
                {
                    int rowIndex = 0;
                    for (int i = 0; i < Dt.Rows.Count && Dt.Rows[i].ItemArray[0].ToString().Length > 0; i++)
                    {
                        szAttributeType = Dt.Rows[i].ItemArray[3].ToString();
                        if ((szAttributeType == "0") ||
                            (IsClientSource && szAttributeType == "2") ||
                            (!IsClientSource && szAttributeType == "1")) continue;

                        szLoadText = "if(info.GetType().GetField(\"";
                        szLoadText += Dt.Rows[i].ItemArray[0].ToString();
                        szLoadText += "\") != null) { info.GetType().GetField(\"";
                        szLoadText += Dt.Rows[i].ItemArray[0].ToString();
                        szLoadText += "\").SetValue(info,";

                        

                        szAttribute = Dt.Rows[i].ItemArray[2].ToString();
                        if (szAttribute.ToLower() == "byte")
                        {
                            szLoadText += "Convert.ToByte(node.Attributes[" + rowIndex.ToString() + "].Value)";
                        }
                        else if (szAttribute.ToLower() == "short")
                        {
                            szLoadText += "Convert.ToInt16(node.Attributes[" + rowIndex.ToString() + "].Value)";
                        }
                        else if (szAttribute.ToLower() == "int")
                        {
                            szLoadText += "Convert.ToInt32(node.Attributes[" + rowIndex.ToString() + "].Value)";
                        }
                        else if (szAttribute.ToLower() == "long")
                        {
                            szLoadText += "Convert.ToInt64(node.Attributes[" + rowIndex.ToString() + "].Value)";
                        }
                        else if (szAttribute.ToLower() == "float")
                        {
                            szLoadText += "Convert.ToSingle(node.Attributes[" + rowIndex.ToString() + "].Value)";
                        }
                        else if (szAttribute.ToLower() == "array_byte")
                        {
                            szLoadText += "String.IsNullOrEmpty(node.Attributes[" + rowIndex.ToString() + "].Value) ? new byte[0] : Array.ConvertAll<string,byte>(node.Attributes[" + rowIndex.ToString() + "].Value.Split(','),byte.Parse)";
                        }
                        else if (szAttribute.ToLower() == "array_int")
                        {
                            szLoadText += "String.IsNullOrEmpty(node.Attributes[" + rowIndex.ToString() + "].Value) ? new int[0] : Array.ConvertAll<string,int>(node.Attributes[" + rowIndex.ToString() + "].Value.Split(','),int.Parse)";
                        }
                        else if (szAttribute.ToLower() == "array_float")
                        {
                            szLoadText += "String.IsNullOrEmpty(node.Attributes[" + rowIndex.ToString() + "].Value) ? new float[0] : Array.ConvertAll<string,float>(node.Attributes[" + rowIndex.ToString() + "].Value.Split(','),float.Parse)";
                        }
                        else if (szAttribute.ToLower() == "array_string")
                        {
                            szLoadText += "String.IsNullOrEmpty(node.Attributes[" + rowIndex.ToString() + "].Value) ? new string[0] : node.Attributes[" + rowIndex.ToString() + "].Value.Split(',')";
                        }
                        else if (szAttribute.ToLower() == "list_int")
                        {
                            szLoadText += "String.IsNullOrEmpty(node.Attributes[" + rowIndex.ToString() + "].Value) ? new List<int>() : Array.ConvertAll<string,int>(node.Attributes[" + rowIndex.ToString() + "].Value.Split(','),int.Parse).ToList()";
                        }
                        else if (szAttribute.ToLower() == "list_float")
                        {
                            szLoadText += "String.IsNullOrEmpty(node.Attributes[" + rowIndex.ToString() + "].Value) ? new List<float>() : Array.ConvertAll<string,float>(node.Attributes[" + rowIndex.ToString() + "].Value.Split(','),float.Parse).ToList()";
                        }
                        else if (szAttribute.ToLower() == "list_string")
                        {
                            szLoadText += "String.IsNullOrEmpty(node.Attributes[" + rowIndex.ToString() + "].Value) ? new List<string>() : new List<string>(node.Attributes[" + rowIndex.ToString() + "].Value.Split(','))";
                        }
                        else if (szAttribute.ToLower() == "string")
                        {
                            szLoadText += "node.Attributes[" + rowIndex.ToString() + "].Value";
                        }
                        else if (szAttribute.ToLower().Contains("enum"))
                        {
                            string[] text = szAttribute.Split(' ');

                            szLoadText += " Convert.ToInt32((int)Enum.Parse(typeof(" + text[1] + "), node.Attributes[" + rowIndex.ToString() + "].Value))";

                        }
                        else
                        {
                            szLoadText = "if(info.GetType().GetField(\"";
                            szLoadText += Dt.Rows[i].ItemArray[0].ToString();
                            szLoadText += "\") != null) { info.";
                            szLoadText += Dt.Rows[i].ItemArray[0].ToString();

                            var t = szAttribute;
                            szLoadText += string.Format(" = Newtonsoft.Json.JsonConvert.DeserializeObject<{0}>(node.Attributes[{1}].Value", t, rowIndex.ToString());

                        }
                        /*
                        if (info.GetType().GetField("idx") != null) {
                            Debug.Log($"cdn data {xn.Attributes[1].Value}");
                            info.GetType().GetField("idx").SetValue(info, xmlhelper.GetInt(xn, "idx", 0));
                        }
                        */
                        szLoadText += ");} else UnityEngine.Debug.LogError(\"Exception : ";
                        szLoadText += szStructName + Dt.Rows[i].ItemArray[0].ToString() + "\");";

                        aLines.Add(szLine.Replace("#GET_INFO_MAPPING_BY_REFLECTION2#", szLoadText));
                        rowIndex++;
                    }
                }
                else if (szLine.Contains("#KEY_TYPE#"))
                {
                    string tempLine = szLine;
                    if (szLine.Contains("#STRUCT_NAME#"))
                    {
                         tempLine = szLine.Replace("#STRUCT_NAME#", szStructName);
                    }
                    var type = Dt.Rows[0].ItemArray[2].ToString();
                    if (type.Contains("enum "))
                    {
                        type = type.Replace("enum ","");
                    }
                    else
                    {
                        type = type.ToLower();
                    }
                    aLines.Add(tempLine.Replace("#KEY_TYPE#", type));
                }
                else if (szLine.Contains("#KEY_TYPE_CONVERT#"))
                {
                    var type = Dt.Rows[0].ItemArray[2].ToString();
                    string type_text = "";
                    if (type.ToLower() == "string")
                    {
                        type_text = "row[0].ToString()";
                    }
                    else if (type.ToLower() == "int")
                    {
                        type_text = "Convert.ToInt32(row[0])";
                    }
                    else if(type.Contains("enum"))
                    {
                        var text = type.Split(' ');
                        type_text = "Enum.Parse<" + text[1] + ">(row[0].ToString())";
                    }

                    aLines.Add(szLine.Replace("#KEY_TYPE_CONVERT#", type_text));
                }
                else if (szLine.Contains("#KEY_TYPE_CONVERT_BY_REFLECTION#"))
                {
                    var type = Dt.Rows[0].ItemArray[2].ToString();
                    string type_text = "";
                    if (type.ToLower() == "string")
                    {
                        type_text = "row[0].ToString()";
                    }
                    else if (type.ToLower() == "int")
                    {
                        type_text = "Convert.ToInt32(row[0])";
                    }
                    else if(type.Contains("enum"))
                    {
                        var text = type.Split(' ');
                        type_text = "(" + text[1] + ")Enum.Parse(typeof(" + text[1] + "), row[0].ToString())";
                    }

                    aLines.Add(szLine.Replace("#KEY_TYPE_CONVERT_BY_REFLECTION#", type_text));
                }
		else if (szLine.Contains("#KEY_TYPE_CONVERT_BY_REFLECTION2#"))
                {
                    var type = Dt.Rows[0].ItemArray[2].ToString();
                    string type_text = "";
                    if (type.ToLower() == "string")
                    {
                        type_text = "node.Attributes[0].Value";
                    }
                    else if (type.ToLower() == "int")
                    {
                        type_text = "Convert.ToInt32(node.Attributes[0].Value)";
                    }
                    else if(type.Contains("enum"))
                    {
                        var text = type.Split(' ');
                        type_text = "(" + text[1] + ")Enum.Parse(typeof(" + text[1] + "), node.Attributes[0].Value)";
                    }

                    aLines.Add(szLine.Replace("#KEY_TYPE_CONVERT_BY_REFLECTION2#", type_text));
                }

                else if (szLine.Contains("#STRUCT_NAME#"))
                {
                    aLines.Add(szLine.Replace("#STRUCT_NAME#", szStructName));
                }

                else if (szLine.Contains("#CLASS_NAME#"))
                {
                    aLines.Add(szLine.Replace("#CLASS_NAME#", szClassName));
                }

                else if (szLine.Contains("#XML_PATH#"))
                {
                    if (IsClientSource) aLines.Add(szLine.Replace("#XML_PATH#", szXmlPath4Client));
                    else aLines.Add(szLine.Replace("#XML_PATH#", szXmlPath4Server));
                }

                else if (szLine.Contains("#XML_ENC_PATH#"))
                {
                    if (IsClientSource) aLines.Add(szLine.Replace("#XML_ENC_PATH#", szXmlEncPath4Client));
                }

                else if (szLine.Contains("#XML_NAME#"))
                {
                    aLines.Add(szLine.Replace("#XML_NAME#", FileName.Replace("Manager.cs", "")));
                }

                else aLines.Add(szLine);
            }

            string szSourceFileName = (IsClientSource) ?
                                        m_dataPath + "/DataScripts/" + FileName + "Manager.cs" :
                                        m_dataPath + "/../DataGeneratorForServer/ServerDataManager_CSharp/" + FileName + "Manager.cs";
            string[] aWriteLines = aLines.ToArray();
            System.IO.File.WriteAllLines(szSourceFileName, aWriteLines, Encoding.GetEncoding("utf-8"));//("ks_c_5601-1987"));
        }

        else
        {
            Debug.LogError("Can't Search DataManagerFormat_CSharp.txt File");
        }
    }

    void WriteNavigator_CSharp(List<string> ClientFileList , List<string> ServerFileList)
    {
        bool isServerCode = false;
        bool isClientCode = false;

        if (System.IO.File.Exists(Application.dataPath + "/Platform/Editor/DataGenerator/DataManagerNavigatorFormat_CSharp.txt"))
        {
            string[] aReadLines = System.IO.File.ReadAllLines(Application.dataPath + "/Platform/Editor/DataGenerator/DataManagerNavigatorFormat_CSharp.txt");
            List<string> aServerLines = new List<string>();
            List<string> aClientLines = new List<string>();

            foreach (string szLine in aReadLines)
            {
                if (szLine.Contains("#SERVER_CODE_START#"))
                {
                    isServerCode = true;
                    continue;
                }

                if (szLine.Contains("#SERVER_CODE_END#"))
                {
                    isServerCode = false;
                    continue;
                }

                if (szLine.Contains("#CLIENT_CODE_START#"))
                {
                    isClientCode = true;
                    continue;
                }

                if (szLine.Contains("#CLIENT_CODE_END#"))
                {
                    isClientCode = false;
                    continue;
                }

                if (szLine.Contains("#SETTING#"))
                {
                    var FileList = (isClientCode) ? ClientFileList : ServerFileList;

                    foreach (string FileName in FileList)
                    {
                        // 파일 이름 예외 처리
                        if (!string.IsNullOrEmpty(m_szGoogleDriveFileNamePrefix)
                            && FileName.LastIndexOf(m_szGoogleDriveFileNamePrefix) < 0
                            || FileName.Contains("Enum"))
                        {
                            continue;
                        }

                        // 임시 저장 파일 예외 처리
                        if (FileName.LastIndexOf("~$") >= 0)
                        {
                            continue;
                        }

                        if (isClientCode)
                        {
                            aClientLines.Add("\t\tif(CheckAdd(\"" + FileName + "\")) Navigator.Add(typeof(" + FileName + "Manager), " + FileName +
                                             "Manager.Instance);");
                        }

                        if (isServerCode)
                        {
                            aServerLines.Add("\t\t\t\tif(typeof(T).Name==\"" + FileName + "Manager\") { " + FileName +
                                             "Manager.Instance.Load(); Cache.Set<T>(typeof(T).Name, (T)Convert.ChangeType(" +
                                             FileName + "Manager.Instance, typeof(T)), new FileCacheDependency(" +
                                             FileName + "Manager.Instance.GetXMLPath())); }");
                        }
                    }
                }
                else if (szLine.Contains("#SETTING2#"))
                {
                    var FileList = ClientFileList;
                    int cnt = 0;
                    foreach (string FileName in FileList)
                    {
                        // 파일 이름 예외 처리
                        if (!string.IsNullOrEmpty(m_szGoogleDriveFileNamePrefix)
                            && FileName.LastIndexOf(m_szGoogleDriveFileNamePrefix) < 0
                            || FileName.Contains("Enum"))
                        {
                            continue;
                        }

                        // 임시 저장 파일 예외 처리
                        if (FileName.LastIndexOf("~$") >= 0)
                        {
                            continue;
                        }

                        if (cnt == 0) aClientLines.Add("\t\tif(name.Equals(\"" + FileName + "\")) " + FileName + "Manager.Instance.Load();");
                        else aClientLines.Add("\t\telse if(name.Equals(\"" + FileName + "\")) " + FileName + "Manager.Instance.Load();");
                        cnt++;
                    }
                }
                else if (szLine.Contains("#CACHEALL#") && isServerCode)
                {
                    foreach (string FileName in ServerFileList)
                    {
                        // 파일 이름 예외 처리
                        if (!string.IsNullOrEmpty(m_szGoogleDriveFileNamePrefix) && FileName.LastIndexOf(m_szGoogleDriveFileNamePrefix) < 0 || FileName.Contains("Enum")) continue;

                        // 임시 저장 파일 예외 처리
                        if (FileName.LastIndexOf("~$") >= 0) continue;

                        aServerLines.Add("\t\tGetManager<" + FileName + "Manager>();");
                    }
                }
                else
                {
                    if (isServerCode) aServerLines.Add(szLine);
                    else if (isClientCode) aClientLines.Add(szLine);
                    else
                    {
                        aServerLines.Add(szLine);
                        aClientLines.Add(szLine);
                    }
                }

                
            }

            string szSourcrFileName = Application.dataPath +  "/DataScripts/DataManagerNavigator.cs";
            string[] aClientWriteLines = aClientLines.ToArray();
            System.IO.File.WriteAllLines(szSourcrFileName, aClientWriteLines, Encoding.GetEncoding("utf-8"));//("ks_c_5601-1987"));

            szSourcrFileName = Application.dataPath + "/../DataGeneratorForServer/ServerDataManager_CSharp/DataManagerNavigator.cs";
            string[] aServerWriteLines = aServerLines.ToArray();
            System.IO.File.WriteAllLines(szSourcrFileName, aServerWriteLines, Encoding.GetEncoding("utf-8"));//("ks_c_5601-1987"));
        }

        else
        {
            Debug.LogError("Can't Search DataManagerNavigatorFormat_CSharp.txt File");
        }
    }

    void SaveXmlInfo(string LastModifiedUser, DateTime LastModifiedDate, int DataCount)
    {
        //var idx = LastModifiedUser.IndexOf("(");
        //var count = LastModifiedUser.Length - idx;
        //LastModifiedUser = LastModifiedUser.Remove(idx, count);

        MemoryStream xmlStream = new MemoryStream();

        XmlWriter Writer = XmlWriter.Create(xmlStream);
        Writer.WriteStartDocument();

        Writer.WriteWhitespace("\n");
        Writer.WriteStartElement("Data");

        Writer.WriteWhitespace("\n");
        Writer.WriteStartElement("Info");

        Writer.WriteAttributeString("LastModifiedUser", LastModifiedUser);
        Writer.WriteAttributeString("LastModifiedDate", LastModifiedDate.ToString("yyyyMMddHHmmss"));
        Writer.WriteAttributeString("DataCount", DataCount.ToString());
        Writer.WriteEndElement();

        Writer.WriteWhitespace("\n");
        Writer.WriteEndElement();

        Writer.WriteEndDocument();

        Writer.Flush();
        Writer.Close();

        string szFileName = Application.dataPath + "/StreamingAssets/xmlData/info.xml";

        FileStream outStream = new FileStream(szFileName, FileMode.Create, FileAccess.Write);

        xmlStream.WriteTo(outStream);

        xmlStream.Flush();
        xmlStream.Close();

        outStream.Flush();
        outStream.Close();

        System.IO.File.Copy(szFileName, Application.dataPath + "/../Resources/xeData/info.xml", true);
        System.IO.File.Copy(szFileName, Application.dataPath + "/../DataGeneratorForServer/xmlDataForServer/info.xml", true);
    }

    static bool Validator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; }

    public class customDT
    {
        public Dictionary<string, string> column;
    }

    private Dictionary<string, string> cacheFolderName = new Dictionary<string, string>();

    public string GetFolderName(DriveService service, string Id)
    {

        if (cacheFolderName.ContainsKey(Id))
        {
            return cacheFolderName[Id];
        }

        var request = service.Files.Get(Id);
        request.SupportsAllDrives = true;
        request.Fields = "name";
        var file = request.Execute();

        cacheFolderName.Add(Id, file.Name);

        return file.Name;
    }

    public string CreateFolder(DriveService service, string FolderName, string ParentId = null)
    {
        Google.Apis.Drive.v3.Data.File FileMetaData = new Google.Apis.Drive.v3.Data.File();
        FileMetaData.Name = FolderName;
        FileMetaData.MimeType = "application/vnd.google-apps.folder";
        if (ParentId != null)
        {
            FileMetaData.Parents = new List<string>() {ParentId};
        }
        else
        {
            FileMetaData.Parents = new List<string>() { teamdrive_folder_id };
        }
        
        var request = service.Files.Create(FileMetaData);
        request.Fields = "id";
        request.SupportsAllDrives = true;
        var file = request.Execute();

        return file.Id;
    }

    private int copy_count = 0;
    public string CopyFile(DriveService service, String fileId, String folderId, string folderName = "")
    {
        // Retrieve the existing parents to remove
        Google.Apis.Drive.v3.FilesResource.GetRequest getRequest = service.Files.Get(fileId);
        getRequest.Fields = "parents, name";
        getRequest.SupportsAllDrives = true;
        Google.Apis.Drive.v3.Data.File ori_file = getRequest.Execute();

        // Copy the file to the new folder
        var body = new Google.Apis.Drive.v3.Data.File();
        body.Name = ori_file.Name;
        body.Parents = new List<string>() {folderId};
        var updateRequest = service.Files.Copy(body, fileId);
        updateRequest.Fields = "id, parents";
        updateRequest.SupportsAllDrives = true;
        var file = updateRequest.Execute();
        if (file != null)
        {
            Debug.Log("Copy Success : " + folderName + "/" + ori_file.Name);
            copy_count++;
        }
        else
        {
            Debug.Log("Copy Fail : " + ori_file.Name);
        }

        return file.Id;
    }

    public void CopyFolder()
    {
        if (string.IsNullOrEmpty(copy_folder_name))
        {
            throw new Exception("copy folder name is empty");
        }
        
        Debug.Log("Ori Folder : " + m_szGoogleDriveFolderName + " / Copy Folder : " + copy_folder_name);

        if (m_szGoogleDriveFolderName == copy_folder_name)
        {
            throw new Exception("ori == copy folder");
        }

        Stopwatch sw = new Stopwatch();
        sw.Start();

        string[] Scopes = { DriveService.Scope.Drive };
        string ApplicationName = "Datagenerator";

        


        var service_text_json = EditorGUIUtility.Load("Assets/Resources/Config/credentials_service.json") as TextAsset;
        var credential = GoogleCredential.FromJson(service_text_json.text).CreateScoped(DriveService.ScopeConstants.Drive);



        //var text_json = EditorGUIUtility.Load("Assets/Resources/Config/credentials.json") as TextAsset;
        //Stream stream = new MemoryStream(text_json.bytes);

        //// The file token.json stores the user's access and refresh tokens, and is created
        //// automatically when the authorization flow completes for the first time.
        //string credPath = "token.json";
        //var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
        //    GoogleClientSecrets.Load(stream).Secrets,
        //    Scopes,
        //    "user",
        //    CancellationToken.None,
        //    new FileDataStore(credPath, true)).Result;



        // Google Drive
        var drive_service = new Google.Apis.Drive.v3.DriveService(new Google.Apis.Services.BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
        

        
        //복사할 폴더 파일들 가져오기
        var request = drive_service.Files.List();
        request.Spaces = "drive";
        request.Q = "'" + teamdrive_folder_id + "' in parents and mimeType = 'application/vnd.google-apps.folder' and name = '" + m_szGoogleDriveFolderName + "' and trashed=false";
        request.Fields = "files(id,name)";
        request.IncludeTeamDriveItems = true;
        request.SupportsTeamDrives = true;
        Google.Apis.Drive.v3.Data.FileList fileList = request.Execute();

        if (fileList.Files.Count == 0)
        {
            throw new Exception("Origin Folder Not Exists");
        }

        var FolderId = fileList.Files[0].Id;

        request.Spaces = "drive";
        request = drive_service.Files.List();
        request.Q = "'" + FolderId + "' in parents and mimeType='application/vnd.google-apps.spreadsheet' and trashed=false";
        request.Fields = "files(id,name,parents,modifiedTime,lastModifyingUser)";
        request.IncludeTeamDriveItems = true;
        request.SupportsTeamDrives = true;
        fileList = request.Execute();

        request = drive_service.Files.List();
        request.Spaces = "drive";
        request.Q = "'" + FolderId + "' in parents and mimeType='application/vnd.google-apps.folder' and trashed=false";
        request.Fields = "files(id,name)";
        request.IncludeTeamDriveItems = true;
        request.SupportsTeamDrives = true;
        var subFolderList = request.Execute();

        Dictionary<string, string> subFolderNameDic = new Dictionary<string, string>();

        foreach (var iter in subFolderList.Files)
        {
            subFolderNameDic.Add(iter.Id, iter.Name);
            request.Spaces = "drive";
            request = drive_service.Files.List();
            request.Q = "'" + iter.Id + "' in parents and mimeType='application/vnd.google-apps.spreadsheet' and trashed=false";
            request.Fields = "files(id,name,parents)";
            request.IncludeTeamDriveItems = true;
            request.SupportsTeamDrives = true;
            var subFileList = request.Execute();
            foreach (var subFile in subFileList.Files)
            {
                fileList.Files.Add(subFile);
            }
        }

        Debug.Log("Ori File Count : " + fileList.Files.Count);

        //복사할 폴더 생성 (이미 있으면 지우고 생성)
        var copy_folder_request = drive_service.Files.List();
        copy_folder_request.Spaces = "drive";
        copy_folder_request.Q = "mimeType = 'application/vnd.google-apps.folder' and name = '" + copy_folder_name + "' and trashed=false";
        copy_folder_request.Fields = "files(id,name)";
        copy_folder_request.IncludeTeamDriveItems = true;
        copy_folder_request.SupportsTeamDrives = true;
        Google.Apis.Drive.v3.Data.FileList copy_folder_fileList = copy_folder_request.Execute();

        string delete_folder_id = null;
        if (copy_folder_fileList.Files.Count > 0)
        {
            delete_folder_id = copy_folder_fileList.Files[0].Id;
            var trash_request = drive_service.Files.Update(new Google.Apis.Drive.v3.Data.File() {Trashed = true}, delete_folder_id);
            trash_request.SupportsAllDrives = true;
            trash_request.Execute();
            
            //var delete_request = drive_service.Files.Delete(delete_folder_id);
            //delete_request.SupportsAllDrives = true;
            //delete_request.Execute();

            Debug.Log("deleteFolderId : " + delete_folder_id);
        }

        var copyTopFolderId = CreateFolder(drive_service, copy_folder_name, teamdrive_folder_id);

        Debug.Log("copyFolderId : " + copyTopFolderId);

        copy_count = 0;

        Dictionary<string, string> copySubFolderDic = new Dictionary<string, string>();
        foreach (var iter in fileList.Files)
        {

            //Debug.Log("Name : " + iter.Name);
            if (iter.Parents != null)
            {
                if (iter.Parents.Count > 1)
                {
                    Debug.Log("Parent Count - iter.Name : " + iter.Parents.Count);
                }

                foreach (var parent in iter.Parents)
                {
                    if (parent == FolderId)
                    {
                        CopyFile(drive_service, iter.Id, copyTopFolderId);
                        break;
                    }
                    else
                    {
                        var name = subFolderNameDic[parent];
                        //var name = GetFolderName(drive_service, parent);
                        string parentId = null;
                        if (copySubFolderDic.ContainsKey(name))
                        {
                            parentId = copySubFolderDic[name];
                        }
                        else
                        {
                            parentId = CreateFolder(drive_service, name, copyTopFolderId);
                            copySubFolderDic.Add(name, parentId);
                        }
                        CopyFile(drive_service, iter.Id, parentId, name);
                    }
                }
            }
            else
            {
                CopyFile(drive_service, iter.Id, copyTopFolderId);
            }
        }

        if (string.IsNullOrEmpty(delete_folder_id) == false)
        {
            try
            {
                var trash_request = drive_service.Files.Update(new Google.Apis.Drive.v3.Data.File() { Trashed = true }, delete_folder_id);
                trash_request.SupportsAllDrives = true;
                trash_request.Execute();

                Debug.Log("deleteFolderId : " + delete_folder_id);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            
        }

        sw.Stop();
        Debug.Log("Copy File Count : " + copy_count);
        Debug.Log("Elapsed MS : " + sw.ElapsedMilliseconds);
    }

    void GoogleSpreadsheetGenerate()
    {

        DataGeneratorWindow.m_fProgress = 0f;
        DataGeneratorWindow.m_fTotalProgress = 0f;
        DataGeneratorWindow.m_szProgressStatus = "Ready!";

        ServicePointManager.ServerCertificateValidationCallback = Validator;

        // Google Auth
        DataGeneratorWindow.m_szProgressStatus = "Connect Google Drive...";
        //Google.Apis.Auth.OAuth2.UserCredential credential = Google.Apis.Auth.OAuth2.GoogleWebAuthorizationBroker.AuthorizeAsync(
        //       new ClientSecrets
        //       {
        //           ClientId = m_szGoolgeClientID,
        //           ClientSecret = m_szGoogleClientSecret,
        //       },
        //       new[]
        //       {
        //           DriveService.Scope.Drive,
        //           "https://spreadsheets.google.com/feeds/"
        //       },
        //       "client",
        //       CancellationToken.None).Result;

        string[] Scopes = { DriveService.Scope.DriveReadonly };
        string ApplicationName = "Datagenerator";

        //var text_json = EditorGUIUtility.Load("Assets/Resources/Config/credentials.json") as TextAsset;
        //Stream stream = new MemoryStream(text_json.bytes);

        //// The file token.json stores the user's access and refresh tokens, and is created
        //// automatically when the authorization flow completes for the first time.
        //string credPath = "token.json";
        //var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
        //    GoogleClientSecrets.Load(stream).Secrets,
        //    Scopes,
        //    "user",
        //    CancellationToken.None,
        //    new FileDataStore(credPath, true)).Result;

        var service_text_json = EditorGUIUtility.Load("Assets/Resources/Config/credentials_service.json") as TextAsset;
        var credential = GoogleCredential.FromJson(service_text_json.text).CreateScoped(DriveService.ScopeConstants.Drive);


        // Google Drive
        var drive_service = new Google.Apis.Drive.v3.DriveService(new Google.Apis.Services.BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        //Google.Apis.Drive.v2.Data.About about = drive_service.About.Get().Execute();       

        // Get Google Drive Folder
        var request = drive_service.Files.List();
        request.Spaces = "drive";
        request.Q = "'" + teamdrive_folder_id + "' in parents and mimeType = 'application/vnd.google-apps.folder' and name = '" + m_szGoogleDriveFolderName + "' and trashed=false";
        request.Fields = "files(id,name)";
        request.IncludeTeamDriveItems = true;
        request.SupportsTeamDrives = true;
        Google.Apis.Drive.v3.Data.FileList fileList = request.Execute();

        List<string> titles = new List<string>();
        foreach (var iter in fileList.Files)
        {
            titles.Add(iter.Name);
        }
        if (fileList.Files.Count == 0)
        {
            Debug.LogError("Can't Search Folder in Google Drive");
            return;
        }
        else if (fileList.Files.Count > 1)
        {
            Debug.LogError("Duplicate Name avaiable");
            return;
        }
        string FolderId = fileList.Files[0].Id;

        // Get Goolge Drive Spreessheet Names
        DataGeneratorWindow.m_szProgressStatus = "Collect Spreadsheets in Google Drive...";
        request.Spaces = "drive";
        request = drive_service.Files.List();
        request.Q = "'" + FolderId + "' in parents and mimeType='application/vnd.google-apps.spreadsheet' and trashed=false";
        request.Fields = "files(id,name,modifiedTime,lastModifyingUser)";
        request.IncludeTeamDriveItems = true;
        request.SupportsTeamDrives = true;
        fileList = request.Execute();
        
        request = drive_service.Files.List();
        request.Spaces = "drive";
        request.Q = "'" + FolderId + "' in parents and mimeType='application/vnd.google-apps.folder' and trashed=false";
        request.Fields = "files(id)";
        request.IncludeTeamDriveItems = true;
        request.SupportsTeamDrives = true;
        var subFolderList = request.Execute();

        foreach (var iter in subFolderList.Files)
        {
            request.Spaces = "drive";
            request = drive_service.Files.List();
            request.Q = "'" + iter.Id + "' in parents and mimeType='application/vnd.google-apps.spreadsheet' and trashed=false";
            request.Fields = "files(id,name,modifiedTime,lastModifyingUser)";
            request.IncludeTeamDriveItems = true;
            request.SupportsTeamDrives = true;
            var subFileList = request.Execute();
            foreach (var subFile in subFileList.Files)
            {
                fileList.Files.Add(subFile);
            }
        }

        if (fileList.Files.Count <= 0)
        {
            Debug.LogError("Empty Folder in Google Drive");
            return;
        }

        // Start Generate
        int generatedCount = 0;
        string lastModifiedUser = "";
        DateTime lastModifiedDate = DateTime.MinValue;

        string[] aFileId = new string[fileList.Files.Count];
        string[] aFileNameList = new string[fileList.Files.Count];

        ConcurrentDictionary<string, string> ServerFileList = new ConcurrentDictionary<string, string>();
        ConcurrentDictionary<string, string> ClientFileList = new ConcurrentDictionary<string, string>();

        int i = 0;
        foreach (var item in fileList.Files)
        {
            aFileId[i] = item.Id;
            aFileNameList[i] = item.Name;
            if (item.ModifiedTime != null)
            {
                if (DateTime.Compare(lastModifiedDate, (DateTime) item.ModifiedTime) < 0)
                {
                    lastModifiedDate = (DateTime) item.ModifiedTime;
                    lastModifiedUser = item.LastModifyingUser.DisplayName;
                }
            }

            i++;
        }

        // 구글 스프레드시트 수집
        string[] Scopes2 = { SheetsService.Scope.SpreadsheetsReadonly };

        //UserCredential credential2;

        //var obj2 = EditorGUIUtility.Load("Assets/Resources/Config/credentials.json") as TextAsset;
        //Stream stream2 = new MemoryStream(obj2.bytes);
        //string credPath2 = "token.json";
        //credential2 = GoogleWebAuthorizationBroker.AuthorizeAsync(
        //    GoogleClientSecrets.Load(stream2).Secrets,
        //    Scopes2,
        //    "user",
        //    CancellationToken.None,
        //    new FileDataStore(credPath2, true)).Result;


        var spread_service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        LastUpdateInfos.Clear();
        var last_path = m_dataPath + "/last_update_at.json";
        if (DataGeneratorWindow.force_generate_all == false)
        {
            if (File.Exists(last_path))
            {
                var lastText = File.ReadAllText(last_path);
                var obj = JsonUtility.FromJson<LastUpdateInfoMain>(lastText);
                if (obj.infos != null)
                {
                    foreach (var iter in obj.infos)
                    {
                        var newObj = new LastUpdateInfo()
                        {
                            DateTime = iter.DateTime,
                            Name = iter.Name,
                            IsClient = iter.IsClient,
                            IsServer = iter.IsServer,
                        };
                        LastUpdateInfos.TryAdd(iter.Name, newObj);
                    }
                }

                //LastUpdateInfos = new ConcurrentDictionary<string, DateTime>();
                //LastUpdateInfos = JsonUtility.FromJson<ConcurrentDictionary<string, DateTime>>(lastText);
            }
        }



        Debug.Log("Total Files : " + fileList.Files.Count);
        //var core = SystemInfo.processorCount * 2;
        int tcount = 0;
        
        List<Task> tasks = new List<Task>();

        bool run = true;
        while (run)
        {
            tasks.Clear();
            int errorCount = 0;
            tcount = 0;
            foreach (Google.Apis.Drive.v3.Data.File file in fileList.Files)
            {
                tcount++;
                var ptcount = tcount;
                //var status = ThreadRun(file, spread_service, fileList, aFileNameList, aFileId,
                //    ServerFileList, ClientFileList, ref generatedCount, tcount);

                //if (status == ThreadRunStatus.GSheetError)
                //{
                //    errorCount++;
                //    Thread.Sleep(1000);
                //    break;
                //}
                var task = Task.Factory.StartNew(() => ThreadRun(file, spread_service, fileList, aFileNameList, aFileId,
                    ServerFileList, ClientFileList, ref generatedCount, ptcount, ref errorCount));
                //tcount++;
                //Task.WaitAll(task);

                tasks.Add(task);

                //if (tcount % 50 == 0)
                //{
                //    Thread.Sleep(100000);
                //}

                //if (tasks.Count >= core)
                //{
                //    Debug.Log("Wait Task");
                //    Task.WaitAll(tasks.ToArray());
                //    Debug.Log("Task End");
                //    tasks.Clear();
                //}
            }

            Task.WaitAll(tasks.ToArray());

            if (errorCount > 0)
            {
                Thread.Sleep(1000);
            }
            else if (errorCount == 0)
            {
                break;
            }
        }
        
        

        DataGeneratorWindow.m_szTotalProgressDesc = generatedCount.ToString() + " / " + fileList.Files.Count.ToString();

        

        
        //Generate CSharp Navigator Source
        WriteNavigator_CSharp(ClientFileList.Values.OrderBy(o=>o).ToList(), ServerFileList.Values.OrderBy(o => o).ToList());

        Debug.Log("[rex][DataGenerator] SaveXmlInfo(" + lastModifiedUser + ")");

        //Generate Xml Info
        SaveXmlInfo(lastModifiedUser, lastModifiedDate, aFileNameList.Length);

        var dic = LastUpdateInfos.ToList();
        var saveList = new List<LastUpdateInfo>();
        foreach(var iter in dic)
        {
            saveList.Add(new LastUpdateInfo()
            {
                Name = iter.Key,
                DateTime = iter.Value.DateTime.ToString(),
                IsServer = iter.Value.IsServer,
                IsClient = iter.Value.IsClient,
            });
        }

        var mainObj = new LastUpdateInfoMain();
        mainObj.infos = saveList.ToArray();
        var lastJson = JsonUtility.ToJson(mainObj);

        File.WriteAllText(last_path, lastJson);

        DataGeneratorWindow.m_fProgress = 1f;
        DataGeneratorWindow.m_fTotalProgress = 1f;
        DataGeneratorWindow.m_szProgressStatus = "Finish!";
    }

    void AddToLastUpdateInfo(Google.Apis.Drive.v3.Data.File workfeed, bool isServer, bool isClient)
    {
        var obj = new LastUpdateInfo()
        {
            DateTime = workfeed.ModifiedTime.ToString(),
            Name = workfeed.Name,
            IsServer = isServer,
            IsClient = isClient,
        };
        LastUpdateInfo getValue;
        LastUpdateInfos.TryGetValue(workfeed.Name, out getValue);
        if (LastUpdateInfos.TryUpdate(workfeed.Name, obj, getValue) == false)
        {
            LastUpdateInfos.TryAdd(workfeed.Name, obj);
        }
    }

    bool CheckSkip(Google.Apis.Drive.v3.Data.File workfeed, ConcurrentDictionary<string,string> ServerFileList = null, ConcurrentDictionary<string,string> ClientFileList = null)
    {
        //Debug.Log("force_generate_all : " + DataGeneratorWindow.force_generate_all);
        //if (DataGeneratorWindow.force_generate_all)
        //{
        //    return false;
        //}

        //최종 변경 시간이 같으면 스킵
        LastUpdateInfo lastUpdateDate;
        if (LastUpdateInfos.TryGetValue(workfeed.Name, out lastUpdateDate))
        {
            var intSec = 0;
            if (workfeed.ModifiedTime.HasValue)
            {
                var sec = (Convert.ToDateTime(lastUpdateDate.DateTime) - workfeed.ModifiedTime.Value).TotalSeconds;
                intSec = (int) Math.Floor(Math.Abs(sec));
            }

            if (intSec == 0)
            {
                if (ServerFileList != null && lastUpdateDate.IsServer)
                {
                    ServerFileList.TryAdd(workfeed.Name, workfeed.Name);
                }
                if (ClientFileList != null && lastUpdateDate.IsClient)
                {
                    ClientFileList.TryAdd(workfeed.Name, workfeed.Name);
                }
                //Log_Queue.Enqueue(workfeed.Title.Text + " Skipped");
                //Debug.Log(workfeed.Title.Text + " Skipped");
                return true;
            }
            else
            {
                Log_Queue.Enqueue(workfeed.Name + " Generate / Diff Sec : " + intSec);
                //Debug.Log(workfeed.Title.Text + " Generate / Diff Sec : " + intSec);
            }            
        }
        else
        {
            Log_Queue.Enqueue(workfeed.Name + " Generate New");
            //Debug.Log(workfeed.Title.Text + " Generate New");
        }

        return false;
    }

    public enum ThreadRunStatus
    {
        Success,
        Skip,
        Error,
        GSheetError,
    }
    ThreadRunStatus ThreadRun(Google.Apis.Drive.v3.Data.File file, 
        SheetsService service,
        Google.Apis.Drive.v3.Data.FileList fileList,
        string[] aFileNameList,
        string[] aFileId,
        ConcurrentDictionary<string,string> ServerFileList, ConcurrentDictionary<string,string> ClientFileList, ref int generatedCount, int tcount, ref int errorCount)
    {
        if (!string.IsNullOrEmpty(m_szGoogleDriveFileNamePrefix) && file.Name.Contains(m_szGoogleDriveFileNamePrefix) == false)
        {
            return ThreadRunStatus.Skip;
        }

        if (file.Name.Contains("Enum") || file.Name.Contains("CSV"))
        {
            if (CheckSkip(file))
            {
                return ThreadRunStatus.Skip;
            }
        }
        else
        {
            // 파일 체크 
            bool isFind = false;
            for (var f_i = 0; f_i < fileList.Files.Count; f_i++)
            {
                if (file.Name == aFileNameList[f_i] && file.Id == aFileId[f_i])
                {
                    isFind = true;
                    DataGeneratorWindow.m_szTotalFileList.Push(file.Name);
                    DataGeneratorWindow.m_szTotalFileIDList.Push(aFileId[f_i]);
                    break;
                }
            }

            if (!isFind) return ThreadRunStatus.Skip;

            if (CheckSkip(file, ServerFileList, ClientFileList))
            {

                return ThreadRunStatus.Skip;
            }
        }

        //SpreadsheetsResource.GetRequest requestSpread = service.Spreadsheets.Get(file.Id);

        //Spreadsheet sheet = requestSpread.Execute();

        //return;

        
        Debug.Log(tcount + " get sheet : " + file.Name);


        // 파일 이름 예외 처리
        if (file.Name.Contains("Enum"))
        {
            var enum_request = service.Spreadsheets.Values.Get(file.Id, "Data");

            ValueRange enum_response = null;
            try
            {
                enum_response = enum_request.Execute();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Interlocked.Increment(ref errorCount);
                return ThreadRunStatus.GSheetError;
            }

            DataTable Dt2 = new DataTable();

            List<string> enumTextLine = new List<string>();
            enumTextLine.Add(string.Format("public enum {0}", file.Name.Replace("Enum", "")));
            enumTextLine.Add("{");

            int enum_i = 0;
            foreach (var worksheetRow in enum_response.Values)
            {

                var row = Dt2.NewRow();

                int c = 0;
                foreach (var element in worksheetRow)
                {

                    //Debug.Log(element);
                    if (enum_i == 0)
                    {
                        Dt2.Columns.Add(element.ToString());
                        //var column = Dt2.Columns[element.ToString()] ?? Dt2.Columns.Add(element.ToString());
                    }
                    else
                    {
                        //Debug.Log(c + " : " + element);
                        row[c] = element.ToString();
                    }
                    c++;

                }
                if (enum_i > 0)
                {
                    Dt2.Rows.Add(row);
                    enumTextLine.Add(string.Format("\t{0} = {1}, //{2}", row.ItemArray[1], row.ItemArray[0], row.ItemArray[2]));
                }
                enum_i++;
            }
            enumTextLine.Add("}");

            string szSourceFileName = m_dataPath + "/DataScripts/" + file.Name + ".cs";

            string[] aWriteLines = enumTextLine.ToArray();
            //Debug.Log(aWriteLines.Length);
            System.IO.File.WriteAllLines(szSourceFileName, aWriteLines, Encoding.GetEncoding("utf-8"));
            File.Copy(szSourceFileName, m_dataPath + "/../DataGeneratorForServer/ServerDataManager_CSharp/" + file.Name + ".cs", true);

            AddToLastUpdateInfo(file, false, false);

            return ThreadRunStatus.Success;
        }
        else if (file.Name.Contains("CSV"))
        {
            var csv_request = service.Spreadsheets.Values.Get(file.Id, "Data");

            ValueRange csv_response = null;
            try
            {
                csv_response = csv_request.Execute();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Interlocked.Increment(ref errorCount);
                return ThreadRunStatus.GSheetError;
            }

            List<string> csvTextLine = new List<string>();


            DataTable csvDt = new DataTable();


            int csv_i = 0;
            foreach (var worksheetRow in csv_response.Values)
            {

                var row = csvDt.NewRow();

                int c = 0;
                foreach (var element in worksheetRow)
                {

                    try
                    {
                        if (csv_i == 0)
                        {
                            csvDt.Columns.Add(element.ToString());
                        }
                        else
                        {
                            row[c] = element.ToString();

                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(file.Name + " csv_i : " + csv_i + " c : " + c);
                        throw;
                    }

                    c++;

                }
                if (csv_i > 0)
                {
                    csvDt.Rows.Add(row);
                }
                csv_i++;
            }


            for (csv_i = 0; csv_i < csvDt.Rows.Count; csv_i++)
            {
                StringBuilder line = new StringBuilder();
                for (int j = 1; j < csvDt.Columns.Count; j++)
                {
                    var insertString = csvDt.Rows[csv_i].ItemArray[j].ToString();
                    if (insertString.Contains(","))
                    {
                        line.Append("\"").Append(insertString).Append("\"");
                    }
                    else
                    {
                        line.Append(insertString);
                    }
                    if (j < csvDt.Columns.Count - 1)
                    {
                        line.Append(",");
                    }
                }
                csvTextLine.Add(line.ToString());
            }

            Debug.Log(file.Name + " Total Line : " + csvTextLine.Count + 1);

            var fileName = file.Name.Replace("CSV", "");

            string szSourceFileName = m_dataPath + "/../Resources/Text/" + fileName + ".csv";

            string[] aWriteLines = csvTextLine.ToArray();
            System.IO.File.WriteAllLines(szSourceFileName, aWriteLines, Encoding.GetEncoding("utf-8"));
            File.Copy(szSourceFileName, m_dataPath + "/../DataGeneratorForServer/Text/" + fileName + ".csv", true);

            AddToLastUpdateInfo(file, false, false);

            return ThreadRunStatus.Success;
        }



        //DataGeneratorWindow.m_fProgress = 0.2f;
        //DataGeneratorWindow.m_szProgressStatus = "Generating";
        //DataGeneratorWindow.m_szProgressDesc = entry.Title.Text;

        // Load Data
        var request =
            service.Spreadsheets.Values.BatchGet(file.Id);
        request.Ranges = new List<string>() {"Definition", "Data"};

        BatchGetValuesResponse response = null;

        try
        {
            response = request.Execute();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            Interlocked.Increment(ref errorCount);
            return ThreadRunStatus.GSheetError;
        }

        DataTable DefinitionDt = new DataTable();
        DataTable Dt = new DataTable();

        bool IsClientWorksheet = false;
        bool IsServerWorksheet = false;

        try
        {
            foreach (var worksheet in response.ValueRanges)
            {
                if (worksheet.Range.Contains("Definition") == false && worksheet.Range.Contains("Data") == false)
                    continue;

                if (worksheet.Range.Contains("Definition"))
                {
                    int i = 0;

                    foreach (var worksheetRow in worksheet.Values)
                    {

                        var row = DefinitionDt.NewRow();
                        int c = 0;
                        foreach (var element in worksheetRow)
                        {
                            if (i == 0)
                            {
                                DefinitionDt.Columns.Add(element.ToString());
                            }
                            else
                            {
                                row[c] = element.ToString();
                                c++;
                            }



                        }

                        if (i > 0)
                        {
                            if (row[3].ToString() == "1" || row[3].ToString() == "3")
                                IsClientWorksheet = true;

                            if (row[3].ToString() == "2" || row[3].ToString() == "3")
                                IsServerWorksheet = true;

                            DefinitionDt.Rows.Add(row);
                        }

                        i++;
                        //DataGeneratorWindow.m_fProgress = 0.4f;

                    }
                }
                else if (worksheet.Range.Contains("Data"))
                {

                    int i_data = 0;

                    foreach (var worksheetRow in worksheet.Values)
                    {
                        var row = Dt.NewRow();
                        int c = 0;
                        foreach (var element in worksheetRow)
                        {
                            if (i_data == 0)
                            {
                                Dt.Columns.Add(element.ToString());
                            }
                            else
                            {
                                row[c] = element.ToString();
                            }
                            c++;
                        }

                        if (i_data > 0)
                        {
                            Dt.Rows.Add(row);
                        }

                        i_data++;
                        //DataGeneratorWindow.m_fProgress = 0.6f;
                    }
                }
            }
        }
        catch(Exception ex)
        {
            Debug.LogError(file.Name + ":" + ex.Message + "/" + ex.StackTrace);
            throw;
        }

        AddToLastUpdateInfo(file, IsServerWorksheet, IsClientWorksheet);

        if (IsClientWorksheet)
        {
            //Add ClientFileList
            ClientFileList.TryAdd(file.Name, file.Name);

            
            //Generate Xml             
            SaveXmlFile(true, file.Name, DefinitionDt, Dt, file.ModifiedTime.ToString());
            

            
            //Generate CSharp Source
            WriteSource_CSharp(true, file.Name, DefinitionDt, Dt);
            

        }
        //DataGeneratorWindow.m_fProgress = 0.8f;

        if (IsServerWorksheet)
        {
            //Add ClientFileList
            ServerFileList.TryAdd(file.Name, file.Name);

            
            //Generate Xml for Server    
            SaveXmlFile(false, file.Name, DefinitionDt, Dt, file.ModifiedTime.ToString());
            

            
            //Generate CSharp Source
            WriteSource_CSharp(false, file.Name, DefinitionDt, Dt);
            
        }




        //DataGeneratorWindow.m_fProgress = 1f;

        Interlocked.Increment(ref generatedCount);
        //DataGeneratorWindow.m_fTotalProgress = generatedCount / fileList.Items.Count;

        return ThreadRunStatus.Success;
    }

    public void Excute()
    {
        Debug.Log("<<<<<<<<<<<<<<<<Generate Start>>>>>>>>>>>>>>>>>");
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        m_dataPath = Application.dataPath;
        // 클라이언트 Asset Resource 추출폴더 생성
        CreateDirectory("/StreamingAssets");
        CreateDirectory("/StreamingAssets/xmlData");
        CreateDirectory("/StreamingAssets/xeData");

        // 클라이언트 Patch Resource 추출폴더 생성
        CreateDirectory("/../Resources");
        CreateDirectory("/../Resources/xeData");
        // CSV 데이터
        CreateDirectory("/../Resources/Text");

        // 클라이언트 C# Scripts 추출폴더 생성
        CreateDirectory("/DataScripts");

        // 서버데이터 추출풀더 생성
        CreateDirectory("/../DataGeneratorForServer");
        CreateDirectory("/../DataGeneratorForServer/ServerDataManager_CSharp");
        CreateDirectory("/../DataGeneratorForServer/xmlDataForServer");
        CreateDirectory("/../DataGeneratorForServer/Text");


        GoogleSpreadsheetGenerate();

        while (Log_Queue.Count > 0)
        {
            Log_Queue.TryDequeue(out var text);
            Debug.Log(text);
        }

        while (ErrorLog_Queue.Count > 0)
        {
            ErrorLog_Queue.TryDequeue(out var text);
            Debug.LogError(text);
        }

        //AssetDatabase.Refresh();

        var configFile = Application.dataPath + "/Resources/config/config.xml";
        var patchPath = Application.dataPath + "/../resources/config";
        if (!Directory.Exists(patchPath))
            Directory.CreateDirectory(patchPath);

        //File.Copy(Application.dataPath + "/DataScripts/DataGenEnums.cs",
            //Application.dataPath + "/../DataGeneratorForServer/ServerDataManager_CSharp/DataGenEnums.cs", true);
        File.Copy(configFile, patchPath + "/config.xml", true);

        PatchSystem.PatchCreater.Create();

        sw.Stop();
        Debug.Log("<<<<<<<<<<<<<<<<Generate Time : " + sw.ElapsedMilliseconds + ">>>>>>>>>>>>>>>>>");

    }
}

