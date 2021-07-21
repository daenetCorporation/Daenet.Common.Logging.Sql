using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Daenet.Common.Logging.Sql
{
    [ProviderAlias("SqlProvider")]
    public class SqlServerLogProvider : ILoggerProvider, ISupportExternalScope
    {
        private ISqlServerLoggerSettings m_Settings;

        private IExternalScopeProvider _scopeProvider = NullExternalScopeProvider.Instance;

        public Func<string, LogLevel, bool> Filter { get; }

        private readonly ConcurrentDictionary<string, SqlServerLogger> m_Loggers = new ConcurrentDictionary<string, SqlServerLogger>();

        /// <summary>
        /// Creates SQL Logger Provider 
        /// </summary>
        /// <param name="settings">Logger Settings</param>
        /// <param name="filter">TODO..</param>
        public SqlServerLogProvider(ISqlServerLoggerSettings settings)
        {

            this.m_Settings = settings;
        }

        /// <summary>
        /// Creates SQL Logger Provider 
        /// </summary>
        /// <param name="settings">Logger Settings</param>
        /// <param name="filter">TODO..</param>
        public SqlServerLogProvider(IOptions<SqlServerLoggerSettings> settings)/* : this(settings.Value, null)*/
        {
            this.m_Settings = settings.Value;
        }

        /// <summary>
        /// Create SQL Logger
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns>Returns Logger</returns>
        public ILogger CreateLogger(string categoryName)
        {
            return m_Loggers.GetOrAdd(categoryName, createLoggerImplementation);
        }

        private SqlServerLogger createLoggerImplementation(string categoryName)
        {
            return new SqlServerLogger(m_Settings, categoryName, Filter) { ScopeProvider = _scopeProvider };
        }


        public void Dispose()
        {
        }

        public void ForEachScope<TState>(Action<object, TState> callback, TState state)
        {

        }

        public IDisposable Push(object state)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;

            foreach (System.Collections.Generic.KeyValuePair<string, SqlServerLogger> logger in m_Loggers)
            {
                logger.Value.ScopeProvider = _scopeProvider;
            }
        }


    }

    /// <summary>
    /// Scope provider that does nothing.
    /// </summary>
    internal class NullExternalScopeProvider : IExternalScopeProvider
    {
        private NullExternalScopeProvider()
        {
        }

        /// <summary>
        /// Returns a cached instance of <see cref="NullExternalScopeProvider"/>.
        /// </summary>
        public static IExternalScopeProvider Instance { get; } = new NullExternalScopeProvider();

        /// <inheritdoc />
        void IExternalScopeProvider.ForEachScope<TState>(Action<object, TState> callback, TState state)
        {
        }

        /// <inheritdoc />
        IDisposable IExternalScopeProvider.Push(object state)
        {
            return NullScope.Instance;
        }
    }

    /// <summary>
    /// An empty scope without any logic
    /// </summary>
    internal sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();

        private NullScope()
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
