namespace NanoMessageBus.Receiver.Test
{
    using System;
    using System.Threading.Tasks;
    using Handlers;
    using Models;
    using Xunit;

    public class HandlerTest
    {
        [Fact]
        public async Task DummyGuidHandler()
        {
            // arrange
            var message = new DummyGuidMessage();
            var handler = new DummyGuidHandler();

            // act
            await handler.RegisterStatisticsAsync(DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow);
            await handler.BeforeHandleAsync(message);
            await handler.HandleAsync(message);
            await handler.AfterHandleAsync(message);

            // assert
            Assert.True(handler.RegisterStatisticsAsyncPassed);
            Assert.True(handler.BeforeHandlerAsyncPassed);
            Assert.True(handler.HandleAsyncPassed);
            Assert.True(handler.AfterHandleAsyncPassed);
        }

        [Fact]
        public async Task DummyIntHandler()
        {
            // arrange
            var message = new DummyIntMessage();
            var handler = new DummyIntHandler();

            // act
            await handler.RegisterStatisticsAsync(DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow);
            await handler.BeforeHandleAsync(message);
            await handler.HandleAsync(message);
            await handler.AfterHandleAsync(message);

            // assert
            Assert.True(handler.RegisterStatisticsAsyncPassed);
            Assert.True(handler.BeforeHandlerAsyncPassed);
            Assert.True(handler.HandleAsyncPassed);
            Assert.True(handler.AfterHandleAsyncPassed);
        }
    }
}
