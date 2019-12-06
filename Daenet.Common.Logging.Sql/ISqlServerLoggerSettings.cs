using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Daenet.Common.Logging.Sql
{
    /// <summary>
    /// Defines the configuration of SqlServerLogger.
    /// </summary>
    public interface ISqlServerLoggerSettings
    {
        #region General SettingsILogger 

        /// <summary>
        /// Defines the LogLevel for specific sources.
        /// </summary>
        IDictionary<string, LogLevel> Switches { get; set; }

        /// <summary>
        /// Flag if scopes should be included.
        /// </summary>
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

        /// <summary>
        /// Table name in database
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// Specifies the retry policy to be used in communication with SqlServer.
        /// </summary>
        //RetryPolicy RetryPolicy { get; set; }

        /// <summary>
        /// Flag to know if table can be created automatically when there exists no table
        /// </summary>
        bool CreateTblIfNotExist { get; set; }

        /// <summary>
        /// Flag if Logger should throw an exception if logging fails.
        /// </summary>
        bool IgnoreLoggingErrors { get; }

        /// <summary>
        /// Character or text, which separetes scopes. I.E.: ' ', '/', '=>'
        /// </summary>
        string ScopeSeparator { get; set; }

        IList<KeyValuePair<string, string>> ScopeColumnMapping { get; }
        #endregion
    }
}