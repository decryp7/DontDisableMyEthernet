using System;
using System.Diagnostics;

namespace DontDisableMyEthernet.Core.Logging
{
    public sealed class EventLogLogger : ILogger
    {
        private static readonly Lazy<ILogger> lazy = new Lazy<ILogger>(() => new EventLogLogger());

        public static ILogger Instance => lazy.Value;

        private const string LogName = "DontDisableMyEthernet";

        public EventLogLogger()
        {
            if (!EventLog.SourceExists(LogName))
            {
                EventLog.CreateEventSource(LogName, LogName);
            }
        }

        public void Write(string message, LogLevel logLevel = LogLevel.Info)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message));
            }

            EventLog.WriteEntry(LogName, message, logLevel.ToEventLogEntryType());
        }
    }

    public static class LogLevelExtension
    {
        public static EventLogEntryType ToEventLogEntryType(this LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Info:
                    return EventLogEntryType.Information;
                case LogLevel.Warning:
                    return EventLogEntryType.Warning;
                case LogLevel.Error:
                    return EventLogEntryType.Error;
            }

            throw new ArgumentException("Unable to convert LogLevel to EventLogEntryType");
        }
    }
}