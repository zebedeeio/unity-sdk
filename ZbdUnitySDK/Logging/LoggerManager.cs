namespace ZbdUnitySDK.Logging
{
    using System;

    public class LoggerManager
    {

        private static ILogger logger;

        public static ILogger GetLogger(LogType logType, Type t, object output = null)
        {

            if (logger != null)
            {
                return logger;
            }

            switch (logType)
            {
                case LogType.LOG4NET:
                    logger = new Log4netLogger(t);
                    break;
                case LogType.CONSOLE:
                default:
                    logger = new ConsoleLogger();
                    break;
            }

            return logger;
        }

    }
}
