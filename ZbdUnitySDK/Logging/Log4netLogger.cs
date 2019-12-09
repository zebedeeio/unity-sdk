namespace ZbdUnitySDK.Logging
{
    using System;
    using log4net;

    public class Log4netLogger : ILogger
    {

        private ILog logger;

        public Log4netLogger(Type type)
        {
            logger = LogManager.GetLogger(type);
        }

        public void Debug(string mesg)
        {

            logger.Debug(mesg);
        }

        public void Error(string mesg)
        {
            logger.Error(mesg);
        }

        public void Info(string mesg)
        {
            logger.Info(mesg);
        }
    }
}
