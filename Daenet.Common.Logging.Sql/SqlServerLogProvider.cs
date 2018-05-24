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
        private Func<string, LogLevel, bool> m_Filter;
        private readonly ConcurrentDictionary<string, SqlServerLogger> m_Loggers = new ConcurrentDictionary<string, SqlServerLogger>();

        /// <summary>
        /// Creates SQL Logger Provider 
        /// </summary>
        /// <param name="settings">Logger Settings</param>
        /// <param name="filter">TODO..</param>
        public SqlServerLogProvider(ISqlServerLoggerSettings settings, Func<string, LogLevel, bool> filter)
        {
            if (filter == null)
                this.m_Filter = ((category, logLevel) => true);
            else
                this.m_Filter = filter;

            this.m_Settings = settings;
        }

        /// <summary>
        /// Creates SQL Logger Provider 
        /// </summary>
        /// <param name="settings">Logger Settings</param>
        /// <param name="filter">TODO..</param>
        public SqlServerLogProvider(IOptions<SqlServerLoggerSettings> settings) : this(settings.Value, null)
        {
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
            return new SqlServerLogger(m_Settings, categoryName, getFilter(categoryName, m_Settings));
        }

        private Func<string, LogLevel, bool> getFilter(string name, ISqlServerLoggerSettings settings)
        {
            if (m_Filter != null)
            {
                return m_Filter;
            }

            if (settings != null)
            {
                foreach (var prefix in getKeyPrefixes(name))
                {
                    LogLevel level;
                    if (settings.Switches.TryGetValue(prefix, out level))
                    {
                        return (n, l) => l >= level;
                    }
                }
            }

            return (n, l) =>
            false;
        }

        private IEnumerable<string> getKeyPrefixes(string name)
        {
            List<string> names = new List<string>();

            var tokens = name.Split('.');

            names.Add(name);

            string currName = name;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < tokens.Length - 1; i++)
            {
                sb.Append(tokens[i]);
                names.Add(sb.ToString());
                if (i < tokens.Length - 1)
                    sb.Append(".");
            }

            names.Add("Default");

            return names;
        }

        public void Dispose()
        {
        }
    }
}
