using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Daenet.Common.Logging.Sql
{
    /// <summary>
    /// Stores as a snik the last error. becasue the logger should not return an error.
    /// This can be used to check if the logger works correctly or if there are any errors.
    /// All Operations are thread safe.
    /// </summary>
    public static class SqlServerLoggerState
    {
        /// <summary>
        /// The Last which is called.
        /// </summary>
        public static SqlServerLoggerError LastError { get; private set; }

        /// <summary>
        /// Commits the current batch to the database.
        /// </summary>
        /// <returns></returns>
        public static Task CommitBatch()
        {
            return SqlServerLogger.CurrentLogTaskInstance?.WriteToDb();
        }


        /// <summary>
        /// Handle the error. this is not thread save the last one who writes wins.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        internal static void HandleError(string message, Exception ex)
        {
            LastError = new SqlServerLoggerError
            {
                Message = message,
                Exception = ex,
                DateTime = DateTime.UtcNow,
            };

            Debug.WriteLine($"{message} {ex}");
        }
    }

    /// <summary>
    /// An Sql Logger error
    /// </summary>
    public class SqlServerLoggerError
    {
        /// <summary>
        /// The Message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The Exception
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// The DateTime.
        /// </summary>
        public DateTime DateTime { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(Message);
            builder.AppendLine($"Occured: {DateTime}");
            builder.AppendLine(Exception.ToString());
            return builder.ToString();
        }
    }
}
