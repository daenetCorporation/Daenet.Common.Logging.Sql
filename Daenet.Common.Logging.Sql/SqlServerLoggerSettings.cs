using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Daenet.Common.Logging.Sql
{
    public class SqlServerLoggerSettings : ISqlServerLoggerSettings
    {
        public IDictionary<string, LogLevel> Switches { get; set; } = new Dictionary<string, LogLevel>();

        public bool IncludeScopes { get; set; }

        public bool IncludeExceptionStackTrace { get; set; }

        public string ConnectionString { get; set; }

        public string TableName { get; set; }

        public bool CreateTblIfNotExist { get; set; }

        public bool IgnoreLoggingErrors { get; set; }
    }
}
