using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Daenet.Common.Logging.Sql
{
    class SqlServerLoggerScope
    {
        private readonly string _name;
        private readonly object _state;
        //private Dictionary<string, string> _scopeInformation;
        //private string _scope;

        internal SqlServerLoggerScope(string name, object state)
        {
            _name = name;
            _state = state;
        }

        public SqlServerLoggerScope Parent { get; private set; }
        public string Scope { get; set; }
        public Dictionary<string, string> ScopeInformation { get; set; }

        private static AsyncLocal<SqlServerLoggerScope> _value = new AsyncLocal<SqlServerLoggerScope>();
        public static SqlServerLoggerScope Current
        {
            set
            {
                _value.Value = value;
            }
            get
            {
                return _value.Value;
            }
        }

        public static IDisposable Push(string name, object state)
        {
            var temp = Current;
            Current = new SqlServerLoggerScope(name, state);
            Current.Parent = temp;

            return new DisposableScope();
        }

        internal object CurrentValue
        {
            get
            {
                return _state;
            }
        }

        public override string ToString()
        {
            return _state?.ToString();
        }

        internal string GetScopeInformation(out Dictionary<string, string> dictionary, ISqlServerLoggerSettings settings)
        {
            if (ScopeInformation == null)
            {
                dictionary = new Dictionary<string, string>();

                StringBuilder builder = new StringBuilder();
                var current = this;//SqlServerLoggerScope.Current;
                string scopeLog = string.Empty;
                var length = builder.Length;

                while (current != null)
                {
                    if (current.CurrentValue is IEnumerable<KeyValuePair<string, object>>)
                    {
                        foreach (var item in (IEnumerable<KeyValuePair<string, object>>)current.CurrentValue)
                        {
                            var map = settings.ScopeColumnMapping.FirstOrDefault(a => a.Key == item.Key);
                            if (!String.IsNullOrEmpty(map.Key))
                            {
                                dictionary.Add(map.Value, item.Value.ToString());
                            }
                        }
                    }
                    if (length == builder.Length)
                    {
                        scopeLog = $"{settings.ScopeSeparator}{current}";
                    }
                    else
                    {
                        scopeLog = $"{settings.ScopeSeparator}{current} ";
                    }

                    builder.Insert(length, scopeLog);
                    current = current.Parent;

                }
                //return builder.ToString();
                this.Scope = builder.ToString();
                this.ScopeInformation = new Dictionary<string, string>(dictionary);
            }
            else
            {
                dictionary = ScopeInformation;
            }
            return Scope.ToString();
        }

        private class DisposableScope : IDisposable
        {
            public void Dispose()
            {
                Current = Current.Parent;
            }
        }
    }
}
