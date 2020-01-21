using log4net;
using log4net.Repository;
using System;
using System.IO;
using System.Reflection;
using ZbdUnitySDK.Logging;

namespace ZebedeeSDKRun
{
    class Program
    {
        static void Main(string[] args)
        {
            // Display the number of command line arguments.
            IZdbLogger logger = new ConsoleLogger();
            logger.Debug("11 Debug print");
            logger.Info("11 Info level log print");
            logger.Error("11 Error level log print");

            IZdbLogger logger2 = new Log4netLogger(logger.GetType());
            logger2.Debug("2 Debug print");
            logger2.Info("2 Info level log print");
            logger2.Error("2 Error level log print");

            IZdbLogger logger3 = LoggerFactory.GetLogger(LogType.CONSOLE, typeof(Program));
            logger3.Debug("3 Debug print");
            logger3.Info("3 Info level log print");
            logger3.Error("3 Error level log print");

            IZdbLogger logger4 = LoggerFactory.GetLogger(LogType.LOG4NET, typeof(Program));
            logger4.Debug("4 Debug print");
            logger4.Info("4 Info level log print");
            logger4.Error("4 Error level log print");


            IZdbLogger logger5 = LoggerFactory.GetLogger(LogType.UNITY_DEBUG, typeof(Program));
            try
            {
                logger5.Debug("5 Debug print");
                logger5.Info("5 Info level log print");
                logger5.Error("5 Error level log print");
            }catch(Exception e)
            {
                Console.WriteLine(e);
            }

            IZdbLogger logger6 = LoggerFactory.GetLogger();
            logger6.Debug("6 Debug print");
            logger6.Info("6 Info level log print");
            logger6.Error("6 Error level log print");
        }
    }
}
