using Autofac;
using XPricer.Framework;

namespace XPricer.Injection
{
    public static class Startup
    {
        public static IContainer Init()
        {
            var container = new ContainerBuilder();
            container.RegisterType<LoggerConfigurator>().As<ILoggerConfigurator>().SingleInstance();

            return container.Build();
        }
    }
}
