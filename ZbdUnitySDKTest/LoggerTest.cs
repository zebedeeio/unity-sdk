using BTCPayServer.Lightning;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using ZbdUnitySDK.Logging;

namespace ZbdUnitySDKTest
{
    public class LoggerTest
    {
        private readonly ITestOutputHelper output;

        public LoggerTest(ITestOutputHelper testOutputHelper)
        {
            this.output = testOutputHelper;
        }


        [Fact]
        public void Write_Log_Test()
        {
            ILogger logger = LoggerManager.GetLogger(LogType.LOG4NET, this.GetType());

            logger.Info("Test LOG!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            logger.Debug("Test LOG!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

            output.WriteLine("TEST DONE from helper1");

        }


    }
}
