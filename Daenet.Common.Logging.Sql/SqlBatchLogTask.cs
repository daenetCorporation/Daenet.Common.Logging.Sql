using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daenet.Common.Logging.Sql
{
    internal class SqlBatchLogTask
    {
        private ISqlServerLoggerSettings m_Settings;
        private DataTable m_CurrentQueue;
        private int m_BatchSize;
        private const int staticColumnCount = 6; // EventId, Type, Message, TimeStamp, CategoryName, Exception = 6
        private int columnCount;
        List<object[]> CurrentList = new List<object[]>();

        private readonly object lockObject = new object();


        public SqlBatchLogTask(ISqlServerLoggerSettings settings)
        {
            m_Settings = settings;
            m_BatchSize = Convert.ToInt32(m_Settings.BatchSize);

            m_CurrentQueue = new DataTable();

            columnCount = staticColumnCount + m_Settings.ScopeColumnMapping.Count();
        }

        public static SqlBatchLogTask Current { get; set; }

        public void Push<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, string categoryName)
        {
            object[] scopeValues;
            if (SqlServerLoggerScope.Current != null)
                scopeValues = SqlServerLoggerScope.Current.GetScopeInformation(m_Settings);
            else
                scopeValues = new string[m_Settings.ScopeColumnMapping.Count()];
            //var entry = m_CurrentQueue.NewRow();
            //entry["EventId"] = eventId.Id;
            //entry["Type"] = Enum.GetName(typeof(LogLevel), logLevel);
            //entry["Message"] = state.ToString();
            //entry["Timestamp"] = DateTime.UtcNow;
            //entry["CategoryName"] = m_CategoryName;
            //entry["Exception"] = exception == null ? (object)DBNull.Value : exception?.ToString();

            object[] args = new object[columnCount];
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

            //foreach (var mapping in m_Settings.ScopeColumnMapping)
            //{
            //    args[actualColumn] = scopeValues[;
            //    actualColumn++;
            //}

            //foreach (var item in scopeValues)
            //{
            //    args[item.Key] = item.Value;
            //}

            lock (lockObject)
            {
                CurrentList.Add(args);
            }

            //m_CurrentQueue.Rows.Add(entry);

            //if (m_CurrentQueue.Rows.Count >= m_BatchSize)
            //    WriteToDbAsync().Start();

            if (CurrentList.Count >= m_BatchSize)
                WriteToDbAsync().Wait();
        }

        private async Task WriteToDbAsync()
        {
            List<object[]> listToWrite;
            lock (lockObject)
            {
                listToWrite = CurrentList;
                CurrentList = new List<object[]>();
            }

            string connection = m_Settings.ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                //create object of SqlBulkCopy which help to insert  
                using (SqlBulkCopy objbulk = new SqlBulkCopy(con))
                {

                    CustomDataReader customDataReader = new CustomDataReader(listToWrite);
                    //assign Destination table name  
                    objbulk.DestinationTableName = m_Settings.TableName;

                    
                    objbulk.ColumnMappings.Add("0", "EventId");
                    objbulk.ColumnMappings.Add("1", "Type");
                    objbulk.ColumnMappings.Add("2", "Message");
                    objbulk.ColumnMappings.Add("3", "TimeStamp");
                    objbulk.ColumnMappings.Add("4", "CategoryName");
                    objbulk.ColumnMappings.Add("5", "Exception");

                    int actualColumn = staticColumnCount;

                    foreach (var mapping in m_Settings.ScopeColumnMapping)
                    {
                        objbulk.ColumnMappings.Add(actualColumn.ToString(), mapping.Value);
                        actualColumn++;
                    }

                    // TODO: For Additionals Use BaseColumnscount + Addititonals
                    //objbulk.ColumnMappings.Add("RequestId", "RequestId");
                    con.Open();


                    /// TODO: Replace hardcoded mapping with cfg.

                    //insert bulk Records into DataBase.  
                    await objbulk.WriteToServerAsync(customDataReader);
                }
            }
        }

        public async Task RunAsync()
        {
            while (true)
            {

            }
        }
    }
}
