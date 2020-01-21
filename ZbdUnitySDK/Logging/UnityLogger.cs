using System;
using System.Security;

namespace ZbdUnitySDK.Logging
{
    public class UnityLogger : IZdbLogger
    {

        public void Debug(string mesg)
        {
            UnityEngine.Debug.Log(mesg);
        }

        public void Error(string mesg)
        {
            UnityEngine.Debug.LogError(mesg);
        }

        public void Warn(string mesg)
        {
            UnityEngine.Debug.LogWarning(mesg);
        }

        public void Info(string mesg)
        {
            UnityEngine.Debug.Log(mesg); ;
        }

    }
}
