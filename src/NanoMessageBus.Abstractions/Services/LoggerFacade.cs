namespace NanoMessageBus.Abstractions.Services
{
    using System;
    using Interfaces;
    using Microsoft.Extensions.Logging;

    public class LoggerFacade<T> : ILoggerFacade<T>
    {
        public ILogger<T> Logger { get; }

        public LoggerFacade(ILogger<T> logger)
        {
            Logger = logger;
        }

        #region Log Information

        public void LogInformation(EventId eventId, Exception exception, string message, params object[] args)
        {
            Logger.LogInformation(eventId, exception, message, args);
        }

        public void LogInformation(EventId eventId, string message, params object[] args)
        {
            Logger.LogInformation(eventId, message, args);
        }

        public void LogInformation(Exception exception, string message, params object[] args)
        {
            Logger.LogInformation(exception, message, args);
        }

        public void LogInformation(string message, params object[] args)
        {
            Logger.LogInformation(message, args);
        }

        #endregion

        #region Log Warning

        public void LogWarning(EventId eventId, Exception exception, string message, params object[] args)
        {
            Logger.LogWarning(eventId, exception, message, args);
        }

        public void LogWarning(EventId eventId, string message, params object[] args)
        {
            Logger.LogWarning(eventId, message, args);
        }

        public void LogWarning(Exception exception, string message, params object[] args)
        {
            Logger.LogWarning(exception, message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            Logger.LogWarning(message, args);
        }

        #endregion

        #region Log Error

        public void LogError(EventId eventId, Exception exception, string message, params object[] args)
        {
            Logger.LogError(eventId, exception, message, args);
        }

        public void LogError(EventId eventId, string message, params object[] args)
        {
            Logger.LogError(eventId, message, args);
        }

        public void LogError(Exception exception, string message, params object[] args)
        {
            Logger.LogError(exception, message, args);
        }

        public void LogError(string message, params object[] args)
        {
            Logger.LogError(message, args);
        }

        #endregion

        #region Log Critical

        public void LogCritical(EventId eventId, Exception exception, string message, params object[] args)
        {
            Logger.LogCritical(eventId, exception, message, args);
        }

        public void LogCritical(EventId eventId, string message, params object[] args)
        {
            Logger.LogCritical(eventId, message, args);
        }

        public void LogCritical(Exception exception, string message, params object[] args)
        {
            Logger.LogCritical(exception, message, args);
        }

        public void LogCritical(string message, params object[] args)
        {
            Logger.LogCritical(message, args);
        }

        #endregion

        #region Log Trace

        public void LogTrace(EventId eventId, Exception exception, string message, params object[] args)
        {
            Logger.LogTrace(eventId, exception, message, args);
        }

        public void LogTrace(EventId eventId, string message, params object[] args)
        {
            Logger.LogTrace(eventId, message, args);
        }

        public void LogTrace(Exception exception, string message, params object[] args)
        {
            Logger.LogTrace(exception, message, args);
        }

        public void LogTrace(string message, params object[] args)
        {
            Logger.LogTrace(message, args);
        }

        #endregion

        #region Log Debug

        public void LogDebug(EventId eventId, Exception exception, string message, params object[] args)
        {
            Logger.LogDebug(eventId, exception, message, args);
        }

        public void LogDebug(EventId eventId, string message, params object[] args)
        {
            Logger.LogDebug(eventId, message, args);
        }

        public void LogDebug(Exception exception, string message, params object[] args)
        {
            Logger.LogDebug(exception, message, args);
        }

        public void LogDebug(string message, params object[] args)
        {
            Logger.LogDebug(message, args);
        }

        #endregion
    }
}
