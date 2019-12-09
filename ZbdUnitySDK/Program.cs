using BTCPayServer.Lightning;
using NBitcoin;
using System;
using ZbdUnitySDK.Logging;

namespace MainRunner
{
    class Program
    {

        static void Main(string[] args)
        {
            // Display the number of command line arguments.
            ILogger logger = new ConsoleLogger();
            logger.Debug("Debug print");
            logger.Info("Info level log print");
            logger.Error("Error level log print");


            ILogger logger2 = new Log4netLogger(logger.GetType());
            logger2.Debug("Debug print");
            logger2.Info("Info level log print");
            logger2.Error("Error level log print");

        }

    }
}


