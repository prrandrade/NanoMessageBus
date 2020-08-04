namespace NanoMessageBus.Abstractions.Interfaces
{
    using System;
    using Microsoft.Extensions.Logging;

    public interface ILoggerFacade<T>
    {
        #region Log Information

        public void LogInformation(EventId eventId, Exception exception, string message, params object[] args);

        public void LogInformation(EventId eventId, string message, params object[] args);

        public void LogInformation(Exception exception, string message, params object[] args);

        public void LogInformation(string message, params object[] args);

        #endregion
        
        #region Log Warning

        public void LogWarning(EventId eventId, Exception exception, string message, params object[] args);

        public void LogWarning(EventId eventId, string message, params object[] args);

        public void LogWarning(Exception exception, string message, params object[] args);

        public void LogWarning(string message, params object[] args);

        #endregion

        #region Log Error

        public void LogError(EventId eventId, Exception exception, string message, params object[] args);

        public void LogError(EventId eventId, string message, params object[] args);

        public void LogError(Exception exception, string message, params object[] args);

        public void LogError(string message, params object[] args);

        #endregion

        #region Log Critical

        public void LogCritical(EventId eventId, Exception exception, string message, params object[] args);

        public void LogCritical(EventId eventId, string message, params object[] args);

        public void LogCritical(Exception exception, string message, params object[] args);

        public void LogCritical(string message, params object[] args);

        #endregion

        #region Log Trace

        public void LogTrace(EventId eventId, Exception exception, string message, params object[] args);

        public void LogTrace(EventId eventId, string message, params object[] args);

        public void LogTrace(Exception exception, string message, params object[] args);

        public void LogTrace(string message, params object[] args);

        #endregion

        #region Log Debug

        public void LogDebug(EventId eventId, Exception exception, string message, params object[] args);

        public void LogDebug(EventId eventId, string message, params object[] args);

        public void LogDebug(Exception exception, string message, params object[] args);

        public void LogDebug(string message, params object[] args);

        #endregion
    }
}
