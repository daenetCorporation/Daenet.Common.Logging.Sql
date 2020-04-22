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
    internal class SqlBatchLogTask : Task
    {
        private ISqlServerLoggerSettings m_Settings;
        private string m_CategoryName;
        private DataTable m_CurrentQueue;
        private int m_BatchSize;
        public SqlBatchLogTask(Action action) : base(action)
        {
            m_BatchSize = Convert.ToInt32(m_Settings.ScopeColumnMapping.FirstOrDefault(s => s.Key == "BatchSize").Value);
        }
        public static SqlBatchLogTask Current { get; set; }

        public void Push<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception)
        {
            Dictionary<string, string> scopeValues;
            if (SqlServerLoggerScope.Current != null)
                scopeValues = SqlServerLoggerScope.Current.GetScopeInformation(m_Settings);
            else
                scopeValues = new Dictionary<string, string>();
            var entry = m_CurrentQueue.NewRow();
            entry["EventId"] = eventId.Id;
            entry["Type"] = Enum.GetName(typeof(LogLevel), logLevel);
            entry["Message"] = state.ToString();
            entry["Timestamp"] = DateTime.UtcNow;
            entry["CategoryName"] = m_CategoryName;
            entry["Exception"] = exception == null ? (object)DBNull.Value : exception?.ToString();

            foreach (var item in scopeValues)
            {
                entry[item.Key] = item.Value;
            }

            m_CurrentQueue.Rows.Add(entry);

            if (m_CurrentQueue.Rows.Count >= m_BatchSize)
                WriteToDbAsync().Start();
        }

        private async Task WriteToDbAsync()
        {
            string connection = m_Settings.ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            //create object of SqlBulkCopy which help to insert  
            SqlBulkCopy objbulk = new SqlBulkCopy(con);

            //assign Destination table name  
            objbulk.DestinationTableName = "SqlLog";

            objbulk.ColumnMappings.Add("EventId", "EventId");
            objbulk.ColumnMappings.Add("Type", "Type");
            objbulk.ColumnMappings.Add("Scope", "Scope");
            objbulk.ColumnMappings.Add("Message", "Message");
            objbulk.ColumnMappings.Add("Exception", "Exception");
            objbulk.ColumnMappings.Add("TimeStamp", "TimeStamp");
            objbulk.ColumnMappings.Add("CategoryName", "CategoryName");
            objbulk.ColumnMappings.Add("RequestId", "RequestId");
            con.Open();

            //insert bulk Records into DataBase.  
            await objbulk.WriteToServerAsync(m_CurrentQueue);
            con.Close();

        } 

        public async Task RunAsync()
        {
            while (true)
            {

            }
        }
    }
}
