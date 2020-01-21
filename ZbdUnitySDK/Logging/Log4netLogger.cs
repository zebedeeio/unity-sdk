namespace ZbdUnitySDK.Logging
{
    using System;
    using System.IO;
    using System.Reflection;
    using log4net;
    using log4net.Appender;
    using log4net.Config;
    using log4net.Core;
    using log4net.Layout;
    using log4net.Repository;
    using log4net.Repository.Hierarchy;
    using ZbdUnitySDK.Exception;

    public class Log4netLogger : IZdbLogger
    {

        private static ILog logger;

        public Log4netLogger(Type type)
        {
            if (logger != null) return;

            //var fileInfo = new FileInfo(@"log4net.config");
            //log4net.Config.XmlConfigurator.Configure(repository, fileInfo);
            ILoggerRepository repository = log4net.LogManager.GetRepository(Assembly.GetCallingAssembly());
            logger = LogManager.GetLogger(type);

            var hierarchy = (Hierarchy)LogManager.GetRepository(repository.Name);
            PatternLayout patternLayout = new PatternLayout
            {
                ConversionPattern = "%date %level %message%newline"
            };
            patternLayout.ActivateOptions();

            var consoleAppender = new ConsoleAppender();
            hierarchy.Root.AddAppender(consoleAppender);
            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;
            BasicConfigurator.Configure(hierarchy);
            
        }

        public void Debug(string mesg)
        {

            logger.Debug(mesg);
        }

        public void Error(string mesg)
        {
            logger.Error(mesg);
        }

        public void Warn(string mesg)
        {
            logger.Warn(mesg);
        }

        public void Info(string mesg)
        {
            logger.Info(mesg);
        }
    }
}
