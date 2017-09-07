using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Daenet.Common.Logging.Sql
{
    /// <summary>
    /// Defines the configuraiton of SqlServerLogger.
    /// </summary>
    public interface ISqlServerLoggerSettings
    {
        #region General SettingsILogger 

        IDictionary<string, LogLevel> Switches { get; set; }

        bool IncludeScopes { get; }

        /// <summary>
        /// If set on true exception stack trace will be logged in a case of an error.
        /// </summary>
        bool IncludeExceptionStackTrace { get; }
        #endregion

        #region SQLLogger specific settings
        /// <summary>
        /// Connection string to EventHub
        /// </summary>
        string ConnectionString { get; }

        string TableName { get; }
        /// <summary>
        /// Specifies the retry policy to be used in commuication with SqlServer.
        /// </summary>
        //RetryPolicy RetryPolicy { get; set; }

        /// <summary>
        /// Flag to know if table can be created automatically when there exists no table
        /// </summary>
        bool CreateTblIfNotExist { get; set; }

        bool IgnoreLoggingErrors { get; }
        #endregion
    }
}