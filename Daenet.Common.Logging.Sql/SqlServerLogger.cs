using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace Daenet.Common.Logging.Sql
{
    public class SqlServerLogger : ILogger
    {
        /// <summary>
        /// Set on true if the Logging fails and it is set on IgnoreLoggingErrors.
        /// </summary>
        private bool m_IsLoggingDisabledOnError = false;

        private bool m_IgnoreLoggingErrors = false;
        private ISqlServerLoggerSettings m_Settings;
        private Func<string, LogLevel, bool> m_Filter;
        private string m_CategoryName;

        internal IExternalScopeProvider ScopeProvider { get; set; }

        private static SqlBatchLogTask currentLogTaskInstance = null;
        private static readonly object padlock = new object();

        private SqlBatchLogTask m_CurrentLogTask
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

//    public Func<LogLevel, EventId, object, Exception, SqlCommand> SqlCommandFormatter { get; set; }

        #region Public Methods

        public SqlServerLogger(ISqlServerLoggerSettings settings, string categoryName, Func<string, LogLevel, bool> filter = null)
        {
            try
            {
                m_Settings = settings;

                if (m_Settings.ScopeSeparator == null)
                    m_Settings.ScopeSeparator = "=>";

                m_CategoryName = categoryName;
                if (filter == null)
                    m_Filter = ((category, logLevel) => true);
                else
                    m_Filter = filter;

                m_IgnoreLoggingErrors = settings.IgnoreLoggingErrors;
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

            m_CurrentLogTask.Push(logLevel, eventId, message, exception, m_CategoryName, ScopeProvider);
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

            if (m_IgnoreLoggingErrors)
            {
                if (m_IsLoggingDisabledOnError == false)
                {
                    SqlServerLoggerErrors.HandleError("Logging has failed and it will be disabled.Error.", ex);
                }

                m_IsLoggingDisabledOnError = true;
            }
            else
                throw new Exception("Ignore Error is disabled.", ex);
        }
        #endregion
    }

}

