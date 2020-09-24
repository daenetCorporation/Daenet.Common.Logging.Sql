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
        /// Timer after all messages in Current Queue will be inserted to database.
        /// No matter if <see cref="BatchSize"/> is reached.
        /// </summary>
        int InsertTimerInSec { get; set; }

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
        /// Flag if Logger should throw an exception if logging fails.
        /// ONLY WORKS IF BATCHSIZE = 1
        /// </summary>
        bool IgnoreLoggingErrors { get; }

        /// <summary>
        /// Character or text, which separetes scopes. I.E.: ' ', '/', '=>'
        /// </summary>
        string ScopeSeparator { get; set; }

        /// <summary>
        /// Defines the size of the buffer. When buffer is full we write all messages to db.
        /// </summary>
        int BatchSize { get; set; }

        IList<KeyValuePair<string, string>> ScopeColumnMapping { get; }

        /// <summary>
        /// Sets the default values for a scope.
        /// </summary>
        IList<KeyValuePair<string, string>> DefaultScopeValues { get; }
        #endregion
    }
}