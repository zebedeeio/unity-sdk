namespace ZbdUnitySDK.Logging
{
    using System;
    using Xunit.Abstractions;

    public class XUnitLogger : IZdbLogger
    {
        private ITestOutputHelper testOutputHelper = null;


        public XUnitLogger(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        public void Debug(string mesg)
        {
            testOutputHelper.WriteLine(mesg);
        }

        public void Error(string mesg)
        {
            testOutputHelper.WriteLine(mesg);
        }

        public void Warn(string mesg)
        {
            testOutputHelper.WriteLine(mesg);
        }

        public void Info(string mesg)
        {
            testOutputHelper.WriteLine(mesg);
        }


    }
}
