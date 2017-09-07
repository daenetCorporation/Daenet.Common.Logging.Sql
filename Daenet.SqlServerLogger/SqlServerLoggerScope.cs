using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Daenet.SqlServerLogger
{
    class SqlServerLoggerScope
    {
        private readonly string _name;
        private readonly object _state;

        internal SqlServerLoggerScope(string name, object state)
        {
            _name = name;
            _state = state;
        }

        public SqlServerLoggerScope Parent { get; private set; }

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

        public override string ToString()
        {
            return _state?.ToString();
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
