using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace ZbdUnitySDK.Logging
{
    class XUnitLogger : ILogger
    {

        private ITestOutputHelper helper;

        public XUnitLogger(object helper)
        {

            this.Helper = (ITestOutputHelper)helper;
        }

        public ITestOutputHelper Helper { get => helper; set => helper = value; }

        public void Debug(string mesg)
        {
            Helper.WriteLine(mesg);
        }

        public void Error(string mesg)
        {
            Helper.WriteLine(mesg);
        }

        public void Info(string mesg)
        {
            Helper.WriteLine(mesg);
        }
    }
}
