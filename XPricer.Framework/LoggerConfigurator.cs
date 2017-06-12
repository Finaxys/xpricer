using NLog;
using NLog.Config;
using NLog.Targets;

namespace XPricer.Framework
{
    public sealed class LoggerConfigurator : ILoggerConfigurator
    {
        private readonly LoggingConfiguration _configuration;

        public LoggerConfigurator()
        {
            _configuration = new LoggingConfiguration();
        }

        public void Configure()
        {
            const string defaultLayout = @"${longdate}|${machinename}${thread}|${level:uppercase=true}|${logger}|${message}";
            var consoleTarget = new ColoredConsoleTarget
            {
                Layout = defaultLayout
            };
            _configuration.AddTarget("console", consoleTarget);


            var fileTarget = new FileTarget()
            {
                Layout = defaultLayout,
                FileName = "${basedir}/logs/xpricer.log",
                ArchiveFileName = "${basedir}/archives/xpricer.{#####}.log",
                ArchiveAboveSize = 10240,
                ArchiveNumbering = ArchiveNumberingMode.Sequence,
                ConcurrentWrites = true,
                KeepFileOpen = false
            };
            _configuration.AddTarget("file", fileTarget);

            // add rules
            var consoleLoggingRule = new LoggingRule("*", LogLevel.Debug, consoleTarget);
            _configuration.LoggingRules.Add(consoleLoggingRule);

            var fileRule = new LoggingRule("*", LogLevel.Trace, fileTarget);
            _configuration.LoggingRules.Add(fileRule);

            LogManager.Configuration = _configuration;
        }
    }
}