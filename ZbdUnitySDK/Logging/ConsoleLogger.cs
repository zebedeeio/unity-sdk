namespace ZbdUnitySDK.Logging
{
    using System;

    public class ConsoleLogger : ILogger
    {

        public void Debug(string mesg)
        {
            ConsoleWrite(mesg);
        }

        public void Error(string mesg)
        {
            ConsoleWrite(mesg);
        }

        public void Info(string mesg)
        {
            ConsoleWrite(mesg);
        }

        private void ConsoleWrite(string mesg)
        {
            Console.WriteLine(mesg);
        }


    }
}
