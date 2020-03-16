namespace ZbdUnitySDK.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public enum LogType
    {
        CONSOLE,
        LOG4NET,
        UNITY_DEBUG,
        XUNIT,    
    }

    public interface IZdbLogger
    {

        void Debug(string mesg);

        void Info(string mesg);

        void Warn(string mesg);

        void Error(string mesg);
    }
}
