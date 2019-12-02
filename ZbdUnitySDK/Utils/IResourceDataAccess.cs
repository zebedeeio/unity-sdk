namespace ZbdUnitySDK.Utils
{
    public interface IResourceDataAccess
    {
        bool FileExists(string relativeFilePath = "poskey.txt");

        string Load(string relativeFilePath = "poskey.txt");

        void Save(string data, string relativeFilePath = "poskey.txt");
    }
}