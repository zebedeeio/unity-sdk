namespace ZbdUnitySDK.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class FileResourceDataAccess : IResourceDataAccess
    {
        public bool FileExists(string relativeFilePath = "poskey.txt")
        {
            return File.Exists(relativeFilePath);
        }

        public string Load(string relativeFilePath = "poskey.txt")
        {
            using (StreamReader sr = new StreamReader(relativeFilePath))
            {
                string line = sr.ReadToEnd();
                return line;
            }
        }

        public void Save(string data, string relativeFilePath = "poskey.txt")
        {
            using (StreamWriter outputFile = new StreamWriter(relativeFilePath))
            {
                    outputFile.Write(data);
            }
        }
    }
}
