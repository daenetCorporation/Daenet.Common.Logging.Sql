using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Daenet.SqlServerLogger
{
    /// <summary>
    /// TODO: Comments on all publics
    /// </summary>
    public static class SqlServerLogProviderExtensions
    {
        /// <summary>
        /// Adds Sql Logger to LoggerFactory
        /// </summary>
        /// <param name="loggerFactory">LoggerFactory Instance</param>
        /// <param name="settings">Sql Logger Settings</param>
        /// <param name="filter">If specified it will override all defined switches.</param>       
        /// <returns></returns>
        public static ILoggerFactory AddSqlServerLogger(this ILoggerFactory loggerFactory,
          ISqlServerLoggerSettings settings,
          Func<string, LogLevel, bool> filter = null)
        {
            loggerFactory.AddProvider(new SqlServerLogProvider(settings, filter));

            return loggerFactory;
        }

        /// <summary>
        /// Adds a SQL Logger to the Logger factory.
        /// </summary>
        /// <param name="loggerFactory">The Logger factory instance.</param>
        /// <param name="config">The .NET Core Configuration for the logger section.</param>
        /// <returns></returns>
        public static ILoggerFactory AddSqlServerLogger(this ILoggerFactory loggerFactory, IConfiguration config)
        {

            loggerFactory.AddProvider(new SqlServerLogProvider(config.GetSqlServerLoggerSettings(), null));

            return loggerFactory;
        }


        /// <summary>
        /// Gets settings from configuration.
        /// </summary>
        /// <param name="config">Configuration for SQL Server Logging.</param>
        /// <returns></returns>
        public static ISqlServerLoggerSettings GetSqlServerLoggerSettings(this IConfiguration config)
        {
            SqlServerLoggerSettings settings = new SqlServerLoggerSettings();
            settings.IncludeScopes = config.GetValue<bool>("IncludeScopes");
            config.GetSection("Switches").Bind(settings.Switches);

            var sqlServerSection = config.GetSection("SqlProvider");
            settings.ConnectionString = sqlServerSection.GetValue<string>("ConnectionString");
            settings.IncludeExceptionStackTrace = sqlServerSection.GetValue<bool>("IncludeExceptionStackTrace");

            settings.TableName = sqlServerSection.GetValue<string>("TableName");
            settings.CreateTblIfNotExist = sqlServerSection.GetValue<bool>("CreateTblIfNotExist");
            settings.IgnoreLoggingErrors = sqlServerSection.GetValue<bool>("IgnoreLoggingErrors");

            return settings;
        }
    }
}
