namespace ZbdUnitySDK.Logging
{
    public enum LogType
    {
        CONSOLE,
        LOG4NET,
        UNITY_DEBUG,
    }

    public interface IZdbLogger
    {

        void Debug(string mesg);

        void Info(string mesg);

        void Warn(string mesg);

        void Error(string mesg);
    }
}
