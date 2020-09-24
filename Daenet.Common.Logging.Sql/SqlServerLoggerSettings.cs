using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Daenet.Common.Logging.Sql
{
    public class SqlServerLoggerSettings : ISqlServerLoggerSettings
    {
        public bool IncludeScopes { get; set; }

        public bool IncludeExceptionStackTrace { get; set; }

        public string ConnectionString { get; set; }

        public string TableName { get; set; }

        public bool IgnoreLoggingErrors { get; set; }

        public string ScopeSeparator { get; set; }

        public IList<KeyValuePair<string, string>> ScopeColumnMapping { get; set; } = new List<KeyValuePair<string, string>>();
   
        public IList<KeyValuePair<string, string>> DefaultScopeValues { get; set; } = new List<KeyValuePair<string, string>>();

        public int BatchSize { get; set; }

        public int InsertTimerInSec { get; set; }
    }
}
