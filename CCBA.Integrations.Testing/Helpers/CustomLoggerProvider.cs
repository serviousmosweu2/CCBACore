using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace CCBA.Integrations.Testing.Helpers
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class CustomLoggerProvider : ILoggerProvider
    {
        public static readonly ConcurrentQueue<string> MessageStack = new ConcurrentQueue<string>();

        public ILogger CreateLogger(string categoryName)
        {
            return new CustomConsoleLogger(categoryName);
        }

        public void Dispose()
        { }

        private class CustomConsoleLogger : ILogger
        {
            private readonly string _categoryName;

            public CustomConsoleLogger(string categoryName)
            {
                _categoryName = categoryName;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (!IsEnabled(logLevel)) return;

                var message = formatter(state, exception).Replace("\n", "").Replace(". ", ".\n");

                MessageStack.Enqueue(message);
                Console.WriteLine($"{DateTime.Now:O} {logLevel}:\n{message}\n");
            }
        }
    }
}