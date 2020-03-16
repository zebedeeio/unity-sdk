namespace ZbdUnitySDK.Logging
{
    using System;

    public class LoggerFactory
    {

        private static IZdbLogger logger;

        public static IZdbLogger GetLogger(LogType logType, Type t)
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
                case LogType.UNITY_DEBUG:
                    logger = new UnityLogger();
                    break;
                case LogType.CONSOLE:
                default:
                    logger = new ConsoleLogger();
                    break;
            }

            return logger;
        }

        public static IZdbLogger GetLogger()
        {
            try
            {
                logger = new UnityLogger();
                logger.Debug("Trying to instantiate Unity Logger");
                return logger;
            }catch(Exception e)
            {
                logger = new ConsoleLogger();
                return logger;
            }
        }
    }
}
