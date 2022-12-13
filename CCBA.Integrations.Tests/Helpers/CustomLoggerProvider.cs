using Microsoft.Extensions.Logging;
using System;

namespace CCBA.Integrations.Tests.Helpers
{
    public class CustomLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new CustomConsoleLogger();
        }

        public void Dispose()
        { }

        private class CustomConsoleLogger : ILogger
        {
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

                Console.WriteLine($"{logLevel}: {formatter(state, exception)}");
            }
        }
    }
}