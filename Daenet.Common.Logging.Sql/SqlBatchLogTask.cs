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

        public void Push<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, string categoryName)
        {
            object[] scopeValues;
            if (SqlServerLoggerScope.Current != null)
                scopeValues = SqlServerLoggerScope.Current.GetScopeInformation(m_Settings);
            else
                scopeValues = new string[m_Settings.ScopeColumnMapping.Count()];

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
                 WriteToDb();
        }

        private void WriteToDb()
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
                using (SqlConnection con = new SqlConnection(m_Settings.ConnectionString))
                {
                    //create object of SqlBulkCopy which help to insert  
                    using (SqlBulkCopy objbulk = new SqlBulkCopy(con))
                    {
                        CustomDataReader customDataReader = new CustomDataReader(listToWrite);
                        objbulk.DestinationTableName = m_Settings.TableName;

                        foreach (var mapping in _sqlBulkCopyColumnMappingList)
                        {
                            objbulk.ColumnMappings.Add(mapping);
                        }

                        con.Open();

                        if (m_BatchSize <= 1) // use sync method if BatchSize is < 1
                            objbulk.WriteToServer(customDataReader);
                        else // use async methodd
                            objbulk.WriteToServerAsync(customDataReader);
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
            if (m_Settings.IgnoreLoggingErrors && m_BatchSize <= 1)
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
            if (m_Settings.InsertTimerInSec == 0 || m_Settings.BatchSize == 0)
                return;

            _insertTimerTask = new Task(() =>
            {
                while (true)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(m_Settings.InsertTimerInSec));

                    WriteToDb(); // Just assign it.
                }
            });

            _insertTimerTask.Start();
        }
    }
}
