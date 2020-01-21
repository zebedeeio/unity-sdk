namespace ZbdUnitySDK.Logging
{
    using System;

    public class ConsoleLogger : IZdbLogger
    {

        public void Debug(string mesg)
        {
            ConsoleWrite(mesg);
        }

        public void Error(string mesg)
        {
            ConsoleWrite(mesg);
        }

        public void Warn(string mesg)
        {
            ConsoleWrite(mesg);
        }

        public void Info(string mesg)
        {
            ConsoleWrite(mesg);
        }

        private void ConsoleWrite(string mesg)
        {
            Console.WriteLine("Console:" + mesg);
        }


    }
}
