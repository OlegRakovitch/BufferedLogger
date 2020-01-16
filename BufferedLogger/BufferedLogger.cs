using Microsoft.Extensions.Logging;
using System;

namespace BufferedLogger
{
    internal class BufferedLogger : ILogger
    {
        readonly ILogger logger;
        readonly QueueProcessor queue;

        public BufferedLogger(ILogger logger, QueueProcessor queue)
        {
            this.queue = queue;
            this.logger = logger;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return logger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logger.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            queue.Enqueue(new LogEntry(logger, logLevel, eventId, state, exception, (s, e) => formatter(state, exception)));
        }
    }
}
