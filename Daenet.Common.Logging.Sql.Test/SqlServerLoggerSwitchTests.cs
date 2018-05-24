using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace Daenet.Common.Logging.Sql.Test
{
    public class SqlServerLoggerSwitchTests
    {
        private ILogger m_Logger;

        public SqlServerLoggerSwitchTests()
        {
            //this.getLogger(null, @"SqlServerLoggerSwitchSettings.json");
        }


        [Fact]
        public void TestFullName()
        {
            ILogger logger = this.getLogger(null, "SqlServerLoggerSettings.json");
            using (logger.BeginScope(Guid.NewGuid()))
            {
                logger.LogTrace(123, "Test Trace Message");
            }
        }

        /// <summary>
        /// Initializes the logger
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="settingsFile"></param>
        private ILogger getLogger(Func<string, LogLevel, bool> filter, string settingsFile)
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(settingsFile);
            var configRoot = builder.Build();

            ILoggerFactory loggerFactory = new LoggerFactory().AddSqlServerLogger(configRoot.GetSqlServerLoggerSettings(), filter);
            ILogger logger = loggerFactory.CreateLogger<SqlServerLoggerSwitchTests>();
            return logger;
        }
    }
}
