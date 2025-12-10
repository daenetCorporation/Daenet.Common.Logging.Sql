using Microsoft.Extensions.Logging;
using System;

namespace Daenet.Common.Logging.Sql
{
    public class SqlServerLogger : ILogger
    {
        /// <summary>
        /// Set on true if the Logging fails and it is set on IgnoreLoggingErrors.
        /// </summary>
        private bool m_IsLoggingDisabledOnError = false;

        /// <summary>
        /// The current settings.
        /// </summary>
        private ISqlServerLoggerSettings m_Settings;

        /// <summary>
        /// The Category name for this specfic logger.
        /// </summary>
        private string m_CategoryName;

        /// <summary>
        /// The Externam Scope provider.
        /// </summary>
        internal IExternalScopeProvider ScopeProvider { get; set; }


        private static SqlBatchLogTask currentLogTaskInstance = null;
        private static readonly object padlock = new object();

        private SqlBatchLogTask CurrentLogTask
        {
            get
            {
                lock (padlock)
                {
                    if (currentLogTaskInstance == null)
                    {
                        currentLogTaskInstance = new SqlBatchLogTask(m_Settings);
                    }
                    return currentLogTaskInstance;
                }
            }
        }

        /// <summary>
        /// Gets a current instance if exists.
        /// </summary>
        internal static SqlBatchLogTask CurrentLogTaskInstance
        {
            get
            {
                return currentLogTaskInstance;
            }
        }

        #region Public Methods

        public SqlServerLogger(ISqlServerLoggerSettings settings, string categoryName, Func<string, LogLevel, bool> filter = null)
        {
            try
            {
                m_Settings = settings;

                if (m_Settings.ScopeSeparator == null)
                    m_Settings.ScopeSeparator = "=>";

                m_CategoryName = categoryName;
            }
            catch (Exception ex)
            {
                handleError(ex);
            }
        }


        /// <summary>
        /// Logs the message to SQL table.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        /// <param name="exceptionFormatter"></param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> exceptionFormatter)
        {
            if (!IsEnabled(logLevel))
                return;
            string message;
            if (exceptionFormatter != null)
            {
                // We need to format the value but we don't wnat to include the exception becasue this is stored seperatly.
                message = exceptionFormatter(state, null) ?? "";
            }
            else
                message = state.ToString() ?? "";

            CurrentLogTask.Push(logLevel, eventId, message, exception, m_CategoryName, ScopeProvider);
        }


        /// <summary>
        /// Begins the scope.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state"></param>
        /// <returns></returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return SqlServerLoggerScope.Push("SqlServerLogger", state);
        }

        /// <summary>
        /// Checks if the logger is enabled for the specified log level and above 
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            if (m_IsLoggingDisabledOnError)
                return false;
            return logLevel != LogLevel.None;

        }

        #endregion

        #region Private Methods

        //TODO: Please test ignore errors if logging fails. See IgnoreLoggingErrors.
        private void handleError(Exception ex)
        {

            if (m_Settings.IgnoreLoggingErrors)
            {
                if (m_IsLoggingDisabledOnError == false)
                {
                    SqlServerLoggerState.HandleError("Logging has failed and it will be disabled.Error.", ex);
                }

                m_IsLoggingDisabledOnError = true;
            }
            else
                throw new Exception("Ignore Error is disabled.", ex);
        }
        #endregion
    }

}

