using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LitJson;

namespace MyToolkit
{
    public class DataConverter
    {
        public static byte[] HexStringToBytes(string hexString)
        {
            hexString = hexString.Trim();
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(2 * i, 2).Trim(), 16);
            }
            return bytes;
        }

        public static string BytesToHexString(byte[] bytes)
        {
            string hexString = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    hexString += bytes[i].ToString("X2");
                }
            }
            return hexString;
        }
    }

    public class FileManager
    {
        public static string GetLocalAppPath(string fileName)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);
        }

        public static void AppendStreamString(string path, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            FileStream file = new FileStream(path, FileMode.Append);
            file.Write(data, 0, data.Length);
            file.Flush();
            file.Close();
            file.Dispose();
        }

        public static void CreatStreamString(string path, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            FileStream file = new FileStream(path, FileMode.Create);
            file.Write(data, 0, data.Length);
            file.Flush();
            file.Close();
            file.Dispose();
        }

        public static void AppendLog(string path, string message)
        {
            string log = DateTime.Now.ToString("yyy-MM-dd HH:mm:ss") + "  " + message + Environment.NewLine;
            AppendStreamString(path, log);
        }
    }

    public class JsonManager
    {
        public static void SaveJsonString(string path, string fileName, object data)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = path + "\\" + fileName + ".json";

            string jsonString = JsonMapper.ToJson(data);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            FileStream file = new FileStream(path, FileMode.Create);
            file.Write(jsonBytes, 0, jsonBytes.Length);//写入
            file.Flush();
            file.Close();
        }

        public static T ReadJsonString<T>(string path, string fileName)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = path + "\\" + fileName + ".json";

                FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader stream = new StreamReader(file);
                T jsonData = JsonMapper.ToObject<T>(stream.ReadToEnd());
                file.Flush();
                file.Close();
                //T jsonData = JsonMapper.ToObject<T>(File.ReadAllText(path));
                return jsonData;
            }
            catch (Exception)
            {

            }
            return default(T);
        }

        public static JsonData ReadSimpleJsonString(string path)
        {
            JsonData jsonData = JsonMapper.ToObject(File.ReadAllText(path));
            return jsonData;
        }
    }

}
