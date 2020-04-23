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
        private string m_CategoryName;
        private DataTable m_CurrentQueue;
        private int m_BatchSize;
        private const int staticColumnCount = 6; // EventId, Type, Message, TimeStamp, CategoryName, Exception = 6
        private int columnCount;
        List<object[]> CurrentList = new List<object[]>();


        public SqlBatchLogTask(ISqlServerLoggerSettings settings)
        {
            m_Settings = settings;
            m_BatchSize = Convert.ToInt32(m_Settings.BatchSize);

            m_CurrentQueue = new DataTable();

            columnCount = staticColumnCount + m_Settings.ScopeColumnMapping.Count();
        }

        public static SqlBatchLogTask Current { get; set; }

        public void Push<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception)
        {
            Dictionary<string, string> scopeValues;
            if (SqlServerLoggerScope.Current != null)
                scopeValues = SqlServerLoggerScope.Current.GetScopeInformation(m_Settings);
            else
                scopeValues = new Dictionary<string, string>();
            //var entry = m_CurrentQueue.NewRow();
            //entry["EventId"] = eventId.Id;
            //entry["Type"] = Enum.GetName(typeof(LogLevel), logLevel);
            //entry["Message"] = state.ToString();
            //entry["Timestamp"] = DateTime.UtcNow;
            //entry["CategoryName"] = m_CategoryName;
            //entry["Exception"] = exception == null ? (object)DBNull.Value : exception?.ToString();

            object[] args = new object[columnCount];
            args[0] = eventId.Id;
            args[1] = Enum.GetName(typeof(LogLevel), logLevel);
            args[2] = state.ToString();
            args[3] = DateTime.UtcNow;
            args[4] = m_CategoryName;
            args[5] = exception == null ? (object)DBNull.Value : exception.ToString();

            int actualColumn = staticColumnCount - 1;
            
            foreach (var item in scopeValues)
            {
                args[item.Key] = item.Value;
            }

            CurrentList.Add(args);
            //m_CurrentQueue.Rows.Add(entry);

            //if (m_CurrentQueue.Rows.Count >= m_BatchSize)
            //    WriteToDbAsync().Start();

            if (CurrentList.Count >= m_BatchSize)
                WriteToDbAsync().Start();
        }

        private async Task WriteToDbAsync()
        {
            var listToWrite = m_CurrentQueue;


            string connection = m_Settings.ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                //create object of SqlBulkCopy which help to insert  
                using (SqlBulkCopy objbulk = new SqlBulkCopy(con))
                {

                    //assign Destination table name  
                    objbulk.DestinationTableName = m_Settings.TableName;

                    
                    objbulk.ColumnMappings.Add("0", "EventId");
                    objbulk.ColumnMappings.Add("Type", "Type");
                    objbulk.ColumnMappings.Add("Scope", "Scope");
                    objbulk.ColumnMappings.Add("Message", "Message");
                    objbulk.ColumnMappings.Add("Exception", "Exception");
                    objbulk.ColumnMappings.Add("TimeStamp", "TimeStamp");

                    objbulk.ColumnMappings.Add("CategoryName", "CategoryName");
                    
                    // TODO: For Additionals Use BaseColumnscount + Addititonals
                    objbulk.ColumnMappings.Add("RequestId", "RequestId");
                    con.Open();

                    
                    /// TODO: Replace hardcoded mapping with cfg.

                    //insert bulk Records into DataBase.  
                    await objbulk.WriteToServerAsync(m_CurrentQueue);
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
