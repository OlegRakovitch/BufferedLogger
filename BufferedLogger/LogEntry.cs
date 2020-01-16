using Microsoft.Extensions.Logging;
using System;

namespace BufferedLogger
{
    internal class LogEntry
    {
        public readonly ILogger Logger;
        public readonly LogLevel LogLevel;
        public readonly EventId EventId;
        public readonly object State;
        public readonly Exception Exception;
        public readonly Func<object, Exception, string> Formatter;

        public LogEntry(ILogger logger, LogLevel logLevel, EventId eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            Logger = logger;
            LogLevel = logLevel;
            EventId = eventId;
            State = state;
            Exception = exception;
            Formatter = formatter;
        }
    }
}
