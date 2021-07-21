using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Daenet.Common.Logging.Sql
{
    [ProviderAlias("SqlProvider")]
    public class SqlServerLogProvider : ILoggerProvider
    {
        private ISqlServerLoggerSettings m_Settings;

        public Func<string, LogLevel, bool> Filter { get; }

        private readonly ConcurrentDictionary<string, SqlServerLogger> m_Loggers = new ConcurrentDictionary<string, SqlServerLogger>();

        /// <summary>
        /// Creates SQL Logger Provider 
        /// </summary>
        /// <param name="settings">Logger Settings</param>
        /// <param name="filter">TODO..</param>
        public SqlServerLogProvider(ISqlServerLoggerSettings settings)
        {

            this.m_Settings = settings;
        }

        /// <summary>
        /// Creates SQL Logger Provider 
        /// </summary>
        /// <param name="settings">Logger Settings</param>
        /// <param name="filter">TODO..</param>
        public SqlServerLogProvider(IOptions<SqlServerLoggerSettings> settings)/* : this(settings.Value, null)*/
        {
            this.m_Settings = settings.Value;
        }

        /// <summary>
        /// Create SQL Logger
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns>Returns Logger</returns>
        public ILogger CreateLogger(string categoryName)
        {
            return m_Loggers.GetOrAdd(categoryName, createLoggerImplementation);
        }

        private SqlServerLogger createLoggerImplementation(string categoryName)
        {
            return new SqlServerLogger(m_Settings, categoryName, Filter);
        }


        public void Dispose()
        {
        }
    }
}
