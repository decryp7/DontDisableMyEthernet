namespace DontDisableMyEthernet.Core.Logging
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public interface ILogger
    {
        void Write(string message, LogLevel logLevel = LogLevel.Info);
    }
}