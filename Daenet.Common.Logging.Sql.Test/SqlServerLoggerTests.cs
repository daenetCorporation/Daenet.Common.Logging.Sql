using System;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Daenet.Common.Logging.Sql.Test
{
    //TODO: Please provide tests for combination of namespaces. Use differnet json setting files.
    /*
     Microsoft.Extensions.Logging.EventHub.Test.EventHubLoggerTests
     EventHubLoggerTests
     EventHub.Test.EventHubLoggerTests
     Default
     */
     // TODO: Scope Tests required. See EventHubLogger
    public class SqlServerLoggerTests
    {
#pragma warning disable SA1308 // Variable names must not be prefixed
        private ILogger m_Logger;
#pragma warning restore SA1308 // Variable names must not be prefixed

        /// <summary>
        /// Initializes default logger.
        /// </summary>
        public SqlServerLoggerTests()
        {
            initializeSqlServerLogger(null);
        }


        /// <summary>
        /// TODO: Describe what does this test does.
        /// </summary>
        [Fact]
        public void SqlLoggingTest()
        {
            this.m_Logger.LogTrace("Test Trace Messages");
            this.m_Logger.LogDebug("Test Debug Messages");
            this.m_Logger.LogInformation("Test Information Messages");
            this.m_Logger.LogWarning("Test Warning Messages");
            this.m_Logger.LogError(new EventId(123, "Error123"), new Exception("new exception", new ArgumentNullException()), "Test Error Message");
            this.m_Logger.LogCritical(new EventId(123, "Critical123"), new Exception("new exception", new ArgumentNullException()), "Test Critical Message");
        }


        /// <summary>
        /// Testing scope of the logger
        /// </summary>
        [Fact]
        public void SqlLoggingScopeTest()
        {
            using (this.m_Logger.BeginScope($"Scope1"))
            {
                this.SqlLoggingTest();

                using (this.m_Logger.BeginScope($"Scope2"))
                {
                    this.SqlLoggingTest();

                    using (this.m_Logger.BeginScope($"Scope3"))
                    {
                        this.SqlLoggingTest();
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the logger
        /// </summary>
        /// <param name="filter">Filter used for logging.</param>
        private void initializeSqlServerLogger(Func<string, LogLevel, bool> filter)
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(@"SqlServerLoggerSettings.json");
            var configRoot = builder.Build();
            ILoggerFactory loggerFactory = new LoggerFactory().AddSqlServerLogger(configRoot.GetSqlServerLoggerSettings(), filter);
            this.m_Logger = loggerFactory.CreateLogger<SqlServerLoggerTests>();
        }
    }
}
