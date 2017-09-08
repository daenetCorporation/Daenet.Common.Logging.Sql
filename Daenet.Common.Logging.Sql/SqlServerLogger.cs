using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Data;

namespace Daenet.Common.Logging.Sql
{
    public class SqlServerLogger : ILogger
    {

        private readonly string m_EventHubName;
        private readonly string m_HostName;
        private readonly string m_SasToken;
        private readonly string m_SubSystem;

        /// <summary>
        /// Set on true if the Logging fails and it is set on IgnoreLoggingErrors.
        /// </summary>
        private bool m_IsLoggingDisabledOnError = false;

        private bool m_IgnoreLoggingErrors = false;
        private ISqlServerLoggerSettings m_Settings;
        private Func<string, LogLevel, bool> m_Filter;
        private string m_CategoryName;

        private ScopeManager m_ScopeManager;

        public Func<LogLevel, EventId, object, Exception, SqlCommand> SqlCommandFormatter { get; set; }

        #region Public Methods

        public SqlServerLogger(ISqlServerLoggerSettings settings, string categoryName, Func<string, LogLevel, bool> filter = null, Func<LogLevel, EventId, object, Exception, SqlCommand> eventDataFormatter = null)
        {
            try
            {
                m_Settings = settings;

                m_CategoryName = categoryName;
                if (filter == null)
                    m_Filter = filter ?? ((category, logLevel) => true);
                else
                    m_Filter = filter;

                m_IgnoreLoggingErrors = settings.IgnoreLoggingErrors;

                using (SqlConnection conn = new SqlConnection(m_Settings.ConnectionString))
                {
                    conn.Open();
                    SqlCommandFormatter = SqlCommandFormatter == null ? defaultSqlCmdFormatter : eventDataFormatter;

                    if (!tableExists(conn))
                    {
                        if (!m_Settings.CreateTblIfNotExist)
                            handleError(new Exception("No table exists. You can enable automatic table creation by setting 'CreateTblIfNotExist' to true"));
                        else
                            createSqlTable(conn);
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                handleError(ex);
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> exceptionFormatter)
        {
            //if (m_ScopeManager != null)
            //    Debug.WriteLine($">>>{this.m_ScopeManager.Current} - {state}");
            //else
            //    Debug.WriteLine($">>>{state}");

            if (!IsEnabled(logLevel))
                return;
            using (SqlConnection conn = new SqlConnection(m_Settings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = SqlCommandFormatter(logLevel, eventId, state, exception);
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (m_Settings.IncludeScopes && this.m_ScopeManager == null)
            {
                this.m_ScopeManager = new ScopeManager(state);
            }

            var scope = m_ScopeManager.Push(state);

            return scope;
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

            return m_Filter(m_CategoryName, logLevel);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Implements default formatter for event data, which will be sent to SQL Server.
        /// </summary>
        /// <param name="logLevel">Logging severity levels</param>
        /// <param name="eventId">Event id of the log</param>
        /// <param name="message">Log message</param>
        /// <param name="exception">Exception in case of error</param>
        /// <returns>SQL command to be inserted in SQL server</returns>
        private SqlCommand defaultSqlCmdFormatter(LogLevel logLevel, EventId eventId, object state, Exception exception)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = $"INSERT INTO {m_Settings.TableName} (Scope, EventId, Type, Message, Timestamp, Exception, CategoryName) VALUES (@Scope, @EventId, @Type, @Message, @Timestamp, @Exception, @CategoryName)";
            //            cmd.Parameters.Add(new SqlParameter("@Scope", this.m_ScopeManager == null ? string.Empty : this.m_ScopeManager.Current));
            cmd.Parameters.Add(new SqlParameter("@Scope", m_ScopeManager == null ? String.Empty : m_ScopeManager.Current));
            cmd.Parameters.Add(new SqlParameter("@EventId", eventId.Id));
            cmd.Parameters.Add(new SqlParameter("@Type", Enum.GetName(typeof(Microsoft.Extensions.Logging.LogLevel), logLevel)));
            cmd.Parameters.Add(new SqlParameter("@Message", state.ToString()));
            cmd.Parameters.Add(new SqlParameter("@Timestamp", DateTime.UtcNow));
            cmd.Parameters.Add(new SqlParameter("@CategoryName", m_CategoryName));
            cmd.Parameters.Add(new SqlParameter("@Exception", exception == null ? string.Empty : exception.ToString()));

            //if (m_ScopeManager != null)
            //    Debug.WriteLine(m_ScopeManager.Current);

            return cmd;
        }

        private string getScopeInformation()
        {
            if (m_Settings.IncludeScopes)
            {
                StringBuilder builder = new StringBuilder(); ;
                var current = SqlServerLoggerScope.Current;
                string scopeLog = string.Empty;

                while (current != null)
                {
                    if (builder.Length == 0)
                        scopeLog = $"{current}";
                    else
                        scopeLog = $"/{current}";

                    builder.Append(scopeLog);
                    current = current.Parent;
                }
                return builder.ToString();
            }
            return "";
        }

        /// <summary>
        /// Checks if the Table Exists
        /// </summary>
        /// <returns>Returns true if table exists in database</returns>
        private bool tableExists(SqlConnection conn)
        {
            try
            {
                string cmdStr = $"IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='{m_Settings.TableName}') SELECT 1 ELSE SELECT 0";
                SqlCommand cmd = new SqlCommand(cmdStr, conn);
                int tblExt = Convert.ToInt32(cmd.ExecuteScalar());
                if (tblExt == 1)
                    return true;
                else
                    return false;

            }
            catch (Exception ex)
            {
                handleError(ex);

                return false;
            }
        }

        /// <summary>
        /// Creates SQL table in database with the table name that is provided in the settings
        /// </summary>
        private void createSqlTable(SqlConnection conn)
        {
            try
            {
                string cmdStr = $"CREATE TABLE {m_Settings.TableName}(Id bigint IDENTITY(1,1) NOT NULL, EventId int NULL, Type nvarchar(12) NOT NULL, Scope nvarchar(max) NULL, Message nvarchar(max) NOT NULL, Exception nvarchar(max) NULL, TimeStamp datetime NOT NULL, CategoryName nvarchar(max) NULL, CONSTRAINT PK_{m_Settings.TableName} PRIMARY KEY CLUSTERED (Id ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON))";
                using (SqlCommand cmd = new SqlCommand(cmdStr, conn))
                    cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                handleError(ex);
            }
        }


        //TODO: Please test ignore errors if logging fails. See IgnoreLoggingErrors.
        private void handleError(Exception ex)
        {

            if (m_IgnoreLoggingErrors)
            {
                if (m_IsLoggingDisabledOnError == false)
                {
                    //TODO - Should log somewhere else later.
                    Debug.WriteLine($"Logging has failed and it will be disabled. Error: {ex}");
                }

                m_IsLoggingDisabledOnError = true;
            }
            else
                throw (ex);
        }
        #endregion
    }

}

