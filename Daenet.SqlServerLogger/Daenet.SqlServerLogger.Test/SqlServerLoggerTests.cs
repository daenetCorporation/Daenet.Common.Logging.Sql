using System;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Daenet.SqlServerLogger.Test
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
        private ILogger m_Logger;

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
            m_Logger.LogTrace("Test Trace Messages");
            m_Logger.LogDebug("Test Debug Messages");
            m_Logger.LogInformation("Test Information Messages");
            m_Logger.LogWarning("Test Warning Messages");
            m_Logger.LogError(new EventId(123, "Error123"), new Exception("new exception", new ArgumentNullException()), "Test Error Message");
            m_Logger.LogCritical(new EventId(123, "Critical123"), new Exception("new exception", new ArgumentNullException()), "Test Critical Message");
        }



        /// <summary>
        /// Testing scope of the logger
        /// </summary>
        [Fact]
        public void SqlLoggingScopeTest()
        {
            using (m_Logger.BeginScope($"Scope Begins : {Guid.NewGuid()}"))
            {
                this.SqlLoggingTest();
            }
        }

        
        /// <summary>
        /// Initializes the logger
        /// </summary>
        /// <param name="filter"></param>
        private void initializeSqlServerLogger(Func<string, LogLevel, bool> filter)
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(@"SqlServerLoggerSettings.json");
            var configRoot = builder.Build();
            ILoggerFactory loggerFactory = new LoggerFactory().AddSqlServerLogger(configRoot.GetSqlServerLoggerSettings(), filter);
            m_Logger = loggerFactory.CreateLogger<SqlServerLoggerTests>();
        }
    }
}
