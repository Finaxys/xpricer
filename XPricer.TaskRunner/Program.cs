using System;
using Autofac;
using NLog;
using XPricer.Framework;
using XPricer.Injection;

namespace XPricer.TaskRunner
{
    class Program
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(typeof(Program));

        static void Main(string[] args)
        {
            try
            {
                var container = Startup.Init();
                var loggerConfigurator = container.Resolve<ILoggerConfigurator>();
                loggerConfigurator.Configure();
                logger.Info("Starting the Task Runner");

            }
            catch (Exception e)
            {
                logger.Error("Task finished in error");
                logger.Error(e);
            }


            logger.Trace("just another message");
            logger.Info("Task finished with success");
        }
    }
}
