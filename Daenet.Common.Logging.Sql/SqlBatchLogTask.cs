using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Daenet.Common.Logging.Sql
{
    internal class SqlBatchLogTask
    {
        private ISqlServerLoggerSettings m_Settings;
        private int m_BatchSize;
        private const int staticColumnCount = 6; // EventId, Type, Message, TimeStamp, CategoryName, Exception = 6
        private int _columnCount;
        List<object[]> CurrentList = new List<object[]>();
        private Task _insertTimerTask;
        private List<SqlBulkCopyColumnMapping> _sqlBulkCopyColumnMappingList;

        private readonly object lockObject = new object();

        public SqlBatchLogTask(ISqlServerLoggerSettings settings)
        {
            m_Settings = settings;
            m_BatchSize = Convert.ToInt32(m_Settings.BatchSize);

            _columnCount = staticColumnCount + m_Settings.ScopeColumnMapping.Count();

            buildColumnMapping();

            RunInsertTimer();
        }

        private void buildColumnMapping()
        {
            _sqlBulkCopyColumnMappingList = new List<SqlBulkCopyColumnMapping>();

            _sqlBulkCopyColumnMappingList.Add(new SqlBulkCopyColumnMapping("0", "EventId"));
            _sqlBulkCopyColumnMappingList.Add(new SqlBulkCopyColumnMapping("1", "Type"));
            _sqlBulkCopyColumnMappingList.Add(new SqlBulkCopyColumnMapping("2", "Message"));
            _sqlBulkCopyColumnMappingList.Add(new SqlBulkCopyColumnMapping("3", "TimeStamp"));
            _sqlBulkCopyColumnMappingList.Add(new SqlBulkCopyColumnMapping("4", "CategoryName"));
            _sqlBulkCopyColumnMappingList.Add(new SqlBulkCopyColumnMapping("5", "Exception"));

            int actualColumn = staticColumnCount;

            foreach (var mapping in m_Settings.ScopeColumnMapping)
            {
                _sqlBulkCopyColumnMappingList.Add(new SqlBulkCopyColumnMapping(actualColumn.ToString(), mapping.Value));
                actualColumn++;
            }
        }

        public void Push<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, string categoryName, IExternalScopeProvider externalScopeProvider)
        {
            object[] scopeValues = GetExternalScopeInformation(externalScopeProvider, m_Settings);
            //if (SqlServerLoggerScope.Current != null)
            //    scopeValues = SqlServerLoggerScope.Current.GetScopeInformation(m_Settings);
            //else
            //{
            //    scopeValues = new string[m_Settings.ScopeColumnMapping.Count()];
            //    // TODOD: Optimize
            //    // Loads the default values for a scope.
            //    foreach (var defaultScope in m_Settings.DefaultScopeValues)
            //    {
            //        var map = m_Settings.ScopeColumnMapping.FirstOrDefault(a => a.Key == defaultScope.Key);
            //        if (!String.IsNullOrEmpty(map.Key))
            //        {
            //            scopeValues[m_Settings.ScopeColumnMapping.IndexOf(map)] = defaultScope.Value;
            //        }
            //    }
            //}
            object[] args = new object[_columnCount];
            args[0] = eventId.Id; // EventId
            args[1] = Enum.GetName(typeof(LogLevel), logLevel); // Type
            args[2] = state.ToString(); // Message
            args[3] = DateTime.UtcNow; // Timestamp
            args[4] = categoryName; // CategoryName
            args[5] = exception == null ? (object)DBNull.Value : exception.ToString(); // Exception

            int actualColumn = staticColumnCount;

            for (int i = 0; i < m_Settings.ScopeColumnMapping.Count(); i++)
            {
                args[actualColumn] = scopeValues[i];
                actualColumn++;
            }

            lock (lockObject)
            {
                CurrentList.Add(args);
            }

            if (CurrentList.Count >= m_BatchSize)
            {
                if (m_BatchSize <= 1)
                {
                    WriteToDb().Wait();
                }
                else
                {
                    var task = new Task(async () =>
                    {
                        await WriteToDb();
                    });
                    task.Start();
                }

            }
        }

        internal string[] GetExternalScopeInformation(IExternalScopeProvider externalScopeProvider, ISqlServerLoggerSettings settings)
        {
            if (settings.ScopeColumnMapping != null || settings.ScopeColumnMapping.Count > 0)
            {
                string[] scopeArray;
                scopeArray = new string[settings.ScopeColumnMapping.Count()];
                var builder = new StringBuilder();
                var current = this;
                var scopeLog = string.Empty;
                var length = builder.Length;

                // TODOD: Optimize
                // Loads the default values for a scope.
                foreach (var defaultScope in settings.DefaultScopeValues)
                {
                    var map = settings.ScopeColumnMapping.FirstOrDefault(a => a.Key == defaultScope.Key);
                    if (!String.IsNullOrEmpty(map.Key))
                    {
                        scopeArray[settings.ScopeColumnMapping.IndexOf(map)] = defaultScope.Value;
                    }
                }

                //Is adding scope path configured
                var addScopePath = !string.IsNullOrEmpty(settings.ScopeColumnMapping.FirstOrDefault(k => k.Key == "SCOPEPATH").Key);

                externalScopeProvider.ForEachScope<object>((scope, state) =>
                {
                    if (scope is IEnumerable<KeyValuePair<string, object>>)
                    {
                        foreach (var item in (IEnumerable<KeyValuePair<string, object>>)scope)
                        {
                            // TODO: For performance reasons we need to remove FirstOrDefault and additional IndexOf call and use only one call.
                            var map = settings.ScopeColumnMapping.FirstOrDefault(a => a.Key == item.Key);
                            if (!String.IsNullOrEmpty(map.Key))
                            {
                                scopeArray[settings.ScopeColumnMapping.IndexOf(map)] = item.Value.ToString();
                            }
                        }
                    }
                    if (addScopePath)
                    {
                        if (length == builder.Length)
                        {
                            scopeLog = $"{settings.ScopeSeparator}{getScopeString(scope)}";
                        }
                        else
                        {
                            scopeLog = $"{settings.ScopeSeparator}{getScopeString(scope)} ";
                        }

                        builder.Insert(length, scopeLog);
                    }
                }, null);
                    
                if (addScopePath)
                {
                    var map = settings.ScopeColumnMapping.FirstOrDefault(a => a.Key == "SCOPEPATH");
                    scopeArray[settings.ScopeColumnMapping.IndexOf(map)] = builder.ToString();
                }

                return scopeArray.ToArray();
            }
            return new string[0];
        }

        /// <summary>
        /// Builds a scops string.
        /// If ToString() is implemented then uses it if it returns a dictionary string.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        private string getScopeString(object current)
        {
            var ret = current.ToString();
            if (ret.Contains("System.Collections.Generic.Dictionary")) // Example //=>System.Collections.Generic.Dictionary`2[System.String,System.Object] =>System.Collections.Generic.Dictionary`2[System.String,System.Object]
            {
                if (current is IEnumerable<KeyValuePair<string, object>>)
                {
                    ret = "{" + string.Join(",", ((IEnumerable<KeyValuePair<string, object>>)current).Select(a => $"{a.Key}->{a.Value}")) + "}";
                }
            }
            return ret;
        }

        private async Task WriteToDb()
        {
            List<object[]> listToWrite;

            lock (lockObject)
            {
                // Don't do anything if list is empty.
                if (CurrentList.Count == 0)
                    return;

                listToWrite = CurrentList;
                CurrentList = new List<object[]>();
            }

            try
            {
                // Supress the transaction from the outside, and activate the async flow.
                using (var scope = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                {
                    using (SqlConnection con = new SqlConnection(m_Settings.ConnectionString))
                    {
                        //create object of SqlBulkCopy which help to insert  
                        using (SqlBulkCopy objbulk = new SqlBulkCopy(con))
                        {
                            //
                            // Map Logs to table.
                            CustomDataReader customDataReader = new CustomDataReader(listToWrite);
                            objbulk.DestinationTableName = m_Settings.TableName;

                            foreach (var mapping in _sqlBulkCopyColumnMappingList)
                            {
                                objbulk.ColumnMappings.Add(mapping);
                            }

                            con.Open();
                            await objbulk.WriteToServerAsync(customDataReader);
                        }
                    }
                }
            }
            catch (InvalidOperationException invalidEx)
            {
                if (invalidEx.Message == "The given ColumnMapping does not match up with any column in the source or destination.")
                {

                    handleError(new Exception($"Missing/Invalid table columns. Required columns: {String.Join(",", _sqlBulkCopyColumnMappingList.Select(d => d.DestinationColumn))}", invalidEx));
                }
                else
                    handleError(invalidEx);
            }
            catch (Exception e)
            {
                handleError(e);
            }
        }

        private void handleError(Exception ex)
        {
            if (m_Settings.IgnoreLoggingErrors || m_BatchSize > 1)
            {
                Debug.WriteLine($"Logging has failed. {ex}");
            }
            else
            {
                Debug.WriteLine($"Ignore Error is disabled and an Exception occured: {ex}");
                throw new Exception("Ignore Error is disabled and an Exception occured.", ex);
            }
        }

        private void RunInsertTimer()
        {

            if (m_Settings.InsertTimerInSec == 0 || m_Settings.BatchSize <= 1)
                return;

            _insertTimerTask = new Task(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(m_Settings.InsertTimerInSec));
                    //Thread.Sleep(TimeSpan.FromSeconds(m_Settings.InsertTimerInSec));

                    await WriteToDb(); // Just assign it.
                }
            });

            _insertTimerTask.Start();
        }
    }
}
