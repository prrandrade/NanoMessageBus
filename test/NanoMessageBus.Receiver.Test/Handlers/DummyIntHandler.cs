namespace NanoMessageBus.Receiver.Test.Handlers
{
    using System;
    using System.Threading.Tasks;
    using Models;
    using Services;

    public class DummyIntHandler : MessageHandlerBase<DummyIntMessage>
    {
        public bool RegisterStatisticsAsyncPassed { get; private set; }
        public bool BeforeHandlerAsyncPassed { get; private set; }
        public bool HandleAsyncPassed { get; private set; }
        public bool AfterHandleAsyncPassed { get; private set; }

        public override async Task RegisterStatisticsAsync(DateTime prepareToSendAt, DateTime sentAt, DateTime receivedAt, DateTime handledAt)
        {
            RegisterStatisticsAsyncPassed = true;
            await base.RegisterStatisticsAsync(prepareToSendAt, sentAt, receivedAt, handledAt);
        }

        public override async Task<bool> BeforeHandleAsync(DummyIntMessage message)
        {
            BeforeHandlerAsyncPassed = true;
            return await base.BeforeHandleAsync(message);
        }

        public override async Task HandleAsync(DummyIntMessage message)
        {
            HandleAsyncPassed = true;
            await base.HandleAsync(message);
        }

        public override async Task AfterHandleAsync(DummyIntMessage message)
        {
            AfterHandleAsyncPassed = true;
            await base.AfterHandleAsync(message);
        }
    }
}
