namespace NanoMessageBus.Abstractions.Test.Services
{
    using System;
    using Abstractions.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class LoggerFacadeTest
    {
        public Mock<ILogger<object>> LoggerMock { get; }
        public LoggerFacade<object> LoggerFacade { get; }

        public LoggerFacadeTest()
        {
            LoggerMock = new Mock<ILogger<object>>();
            LoggerFacade = new LoggerFacade<object>(LoggerMock.Object);
        }

        #region Log Information

        [Fact]
        public void LogInformation_AllParameters()
        {
            // arrange
            var eventId = new EventId();
            var exception = new Exception();
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogInformation(eventId, exception, message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Information,
                eventId,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                exception,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        [Fact]
        public void LogInformation_EventId()
        {
            // arrange
            var eventId = new EventId();
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogInformation(eventId, message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Information,
                eventId,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                null,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        [Fact]
        public void LogInformation_Exception()
        {
            // arrange
            var exception = new Exception();
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogInformation(exception, message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Information,
                (EventId) 0,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                exception,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        [Fact]
        public void LogInformation_Message()
        {
            // arrange
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogInformation(message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Information,
                (EventId) 0,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                null,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        #endregion

        #region Log Warning

        [Fact]
        public void LogWarning_AllParameters()
        {
            // arrange
            var eventId = new EventId();
            var exception = new Exception();
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogWarning(eventId, exception, message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                eventId,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                exception,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        [Fact]
        public void LogWarning_EventId()
        {
            // arrange
            var eventId = new EventId();
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogWarning(eventId, message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                eventId,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                null,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        [Fact]
        public void LogWarning_Exception()
        {
            // arrange
            var exception = new Exception();
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogWarning(exception, message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                (EventId) 0,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                exception,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        [Fact]
        public void LogWarning_Message()
        {
            // arrange
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogWarning(message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                (EventId) 0,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                null,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        #endregion

        #region Log Error

        [Fact]
        public void LogError_AllParameters()
        {
            // arrange
            var eventId = new EventId();
            var exception = new Exception();
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogError(eventId, exception, message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Error,
                eventId,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                exception,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        [Fact]
        public void LogError_EventId()
        {
            // arrange
            var eventId = new EventId();
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogError(eventId, message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Error,
                eventId,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                null,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        [Fact]
        public void LogError_Exception()
        {
            // arrange
            var exception = new Exception();
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogError(exception, message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Error,
                (EventId) 0,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                exception,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        [Fact]
        public void LogError_Message()
        {
            // arrange
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogError(message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Error,
                (EventId) 0,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                null,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        #endregion

        #region Log Critical

        [Fact]
        public void LogCritical_AllParameters()
        {
            // arrange
            var eventId = new EventId();
            var exception = new Exception();
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogCritical(eventId, exception, message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Critical,
                eventId,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                exception,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        [Fact]
        public void LogCritical_EventId()
        {
            // arrange
            var eventId = new EventId();
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogCritical(eventId, message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Critical,
                eventId,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                null,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        [Fact]
        public void LogCritical_Exception()
        {
            // arrange
            var exception = new Exception();
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogCritical(exception, message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Critical,
                (EventId) 0,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                exception,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        [Fact]
        public void LogCritical_Message()
        {
            // arrange
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogCritical(message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Critical,
                (EventId) 0,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                null,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        #endregion

        #region Log Trace

        [Fact]
        public void LogTrace_AllParameters()
        {
            // arrange
            var eventId = new EventId();
            var exception = new Exception();
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogTrace(eventId, exception, message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Trace,
                eventId,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                exception,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        [Fact]
        public void LogTrace_EventId()
        {
            // arrange
            var eventId = new EventId();
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogTrace(eventId, message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Trace,
                eventId,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                null,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        [Fact]
        public void LogTrace_Exception()
        {
            // arrange
            var exception = new Exception();
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogTrace(exception, message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Trace,
                (EventId) 0,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                exception,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        [Fact]
        public void LogTrace_Message()
        {
            // arrange
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogTrace(message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Trace,
                (EventId) 0,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                null,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        #endregion

        #region Log Debug

        [Fact]
        public void LogDebug_AllParameters()
        {
            // arrange
            var eventId = new EventId();
            var exception = new Exception();
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogDebug(eventId, exception, message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Debug,
                eventId,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                exception,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        [Fact]
        public void LogDebug_EventId()
        {
            // arrange
            var eventId = new EventId();
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogDebug(eventId, message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Debug,
                eventId,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                null,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        [Fact]
        public void LogDebug_Exception()
        {
            // arrange
            var exception = new Exception();
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogDebug(exception, message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Debug,
                (EventId) 0,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                exception,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        [Fact]
        public void LogDebug_Message()
        {
            // arrange
            const string message = "message";
            var args = new object[] { "1", "2" };

            Func<object, Type, bool> state = (obj, type) => obj.ToString().CompareTo(message) == 0;

            // act
            LoggerFacade.LogDebug(message, args);

            // assert
            LoggerMock.Verify(x => x.Log(
                LogLevel.Debug,
                (EventId) 0,
                It.Is<It.IsAnyType>((obj, type) => state(obj, type)),
                null,
                It.Is<Func<It.IsAnyType, Exception, string>>((obj, type) => true)));
        }

        #endregion
    }
}
