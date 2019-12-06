using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Daenet.Common.Logging.Sql
{
    /// <summary>
    /// TODO: Comments on all publics
    /// </summary>
    public static class SqlServerLogProviderExtensions
    {

        /// <summary>
        /// Adds a sql logger named 'SqlServerLogger' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        public static ILoggingBuilder AddSqlServerLogger(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, SqlServerLogProvider>();

            return builder;
        }

        /// <summary>
        /// Adds a sql logger named 'SqlServerLogger' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="configure"></param>
        public static ILoggingBuilder AddSqlServerLogger(this ILoggingBuilder builder, Action<SqlServerLoggerSettings> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            builder.AddSqlServerLogger();
            builder.Services.Configure(configure);

            return builder;
        }

        /// <summary>
        /// Adds Sql Logger to LoggerFactory
        /// </summary>
        /// <param name="loggerFactory">LoggerFactory Instance</param>
        /// <param name="settings">Sql Logger Settings</param>
        /// <param name="filter">If specified it will override all defined switches.</param>  
        /// <returns></returns>        
        public static ILoggerFactory AddSqlServerLogger(this ILoggerFactory loggerFactory,
          ISqlServerLoggerSettings settings,
          Func<string, LogLevel, bool> filter = null,
          string scopeSeparator = null)
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
            var settings = new SqlServerLoggerSettings();
            SetSqlServerLoggerSettings(settings, config);
            return settings;
            /*            var settings = new SqlServerLoggerSettings();

                        var sqlServerSection = config.GetSection("SqlProvider");

                        settings.ConnectionString = sqlServerSection.GetValue<string>("ConnectionString");

                        if (String.IsNullOrEmpty(settings.ConnectionString))
                            throw new ArgumentException("SqlProvider:ConnectionString is Null or Empty!", nameof(settings.ConnectionString));

                        settings.IncludeExceptionStackTrace = sqlServerSection.GetValue<bool>("IncludeExceptionStackTrace");

                        settings.TableName = sqlServerSection.GetValue<string>("TableName");

                        if (String.IsNullOrEmpty(settings.TableName))
                            throw new ArgumentException("SqlProvider:TableName is Null or Empty!", nameof(settings.TableName));

                        settings.CreateTblIfNotExist = sqlServerSection.GetValue<bool>("CreateTblIfNotExist");
                        settings.IgnoreLoggingErrors = sqlServerSection.GetValue<bool>("IgnoreLoggingErrors");
                        settings.ScopeSeparator = sqlServerSection.GetValue<string>("ScopeSeparator");

                        return settings;*/
        }

        /// <summary>
        /// Set settings from configuration.
        /// </summary>
        /// <param name="config">Configuration for SQL Server Logging.</param>
        /// <returns></returns>
        public static void SetSqlServerLoggerSettings(this SqlServerLoggerSettings settings, IConfiguration config)
        {
            if (settings == null)
                settings = new SqlServerLoggerSettings();

            var sqlServerSection = config.GetSection("SqlProviderSettings");

            settings.ConnectionString = sqlServerSection.GetValue<string>("ConnectionString");

            if (String.IsNullOrEmpty(settings.ConnectionString))
                throw new ArgumentException("SqlProvider:ConnectionString is Null or Empty!", nameof(settings.ConnectionString));

            settings.IncludeExceptionStackTrace = sqlServerSection.GetValue<bool>("IncludeExceptionStackTrace");

            settings.TableName = sqlServerSection.GetValue<string>("TableName");

            if (String.IsNullOrEmpty(settings.TableName))
                throw new ArgumentException("SqlProvider:TableName is Null or Empty!", nameof(settings.TableName));

            settings.CreateTblIfNotExist = sqlServerSection.GetValue<bool>("CreateTblIfNotExist");
            settings.IgnoreLoggingErrors = sqlServerSection.GetValue<bool>("IgnoreLoggingErrors");
            settings.ScopeSeparator = sqlServerSection.GetValue<string>("ScopeSeparator");

            var columnsMapping = sqlServerSection.GetSection("ScopeColumnMapping");
            if (columnsMapping != null)
            {
                foreach (var item in columnsMapping.GetChildren())
                {
                    settings.ScopeColumnMapping.Add(new KeyValuePair<string, string>(item.Key, item.Value));
                }
            }

        }
    }
}
