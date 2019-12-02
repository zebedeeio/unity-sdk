namespace ZbdUnitySDK.Utils
{
    using System;
    using System.IO;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Supported to use only from Unity Editor
    /// This class handles the file access to Unity Asset folder. Default used for EC privatekey.
    /// </summary>
    public class UnityResourceDataAccess : IResourceDataAccess
    {
        private static string resourcesPath = @"Assets/Resources/";

        private static UTF8Encoding utf8Enc = new UTF8Encoding();

        public bool FileExists(string relativeFilePath = "poskey.txt")
        {
            string extension = System.IO.Path.GetExtension(relativeFilePath);
            string relaivePath = relativeFilePath.Substring(0, relativeFilePath.Length - extension.Length);

            TextAsset textFile = Resources.Load<TextAsset>(relaivePath);
            Debug.Log("ResourcesDataAccess.FileExists() relativeFilePath w/o extension:" + relaivePath + " " + (textFile == null ? "Not exists" : " Exists"));
            return textFile != null;
        }

        // relativeFilePath from resource folder
        public void Save(string data, string relativeFilePath = "poskey.txt")
        {
            string dataPath = resourcesPath + relativeFilePath;
            Debug.Log("ResourcesDataAccess.Save() filesystemPath:" + dataPath + " relativePath in Resources:" + relativeFilePath);

            FileStream fileStream;
            try
            {
                // update existing file
                if (FileExists(relativeFilePath))
                {
                    File.WriteAllText(dataPath, string.Empty);
                    fileStream = File.Open(dataPath, FileMode.Open);
                }

                // a new file
                else
                {
                    fileStream = File.Create(dataPath);
                }

                byte[] bytes = utf8Enc.GetBytes(data);
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Close();
            }
            catch (Exception e)
            {
                Debug.Log("ResourcesDataAccess.Save() Failed:" + e.ToString());
            }
        }

        public string Load(string relativeFilePath = "poskey.txt")
        {
            string extension = System.IO.Path.GetExtension(relativeFilePath);
            string relaivePath = relativeFilePath.Substring(0, relativeFilePath.Length - extension.Length);
            TextAsset textFile = Resources.Load<TextAsset>(Path.GetFileNameWithoutExtension(relativeFilePath));
            string data = null;
            if (textFile != null)
            {
                data = textFile.text;
            }
            else
            {
                Debug.Log("ResourcesDataAccess.Load() File not found in resources : " + relativeFilePath);
            }

            return data;
        }
    }
}
