namespace NanoMessageBus.Receiver.Services
{
    using System;
    using System.Threading.Tasks;
    using Abstractions;
    using Interfaces;

    public abstract class MessageHandlerBase<TMessage> : IMessageHandler<TMessage> where TMessage : IMessage
    {
        public virtual Task RegisterStatistics(long prepareToSendAt, long sentAt, long receivedAt, long handledAt)
        {
            return Task.CompletedTask;
        }

        public virtual Task<bool> BeforeHandle(TMessage message)
        {
            return Task.FromResult(true);
        }

        public virtual Task Handle(TMessage message)
        {
            return Task.CompletedTask;
        }

        public virtual Task AfterHandle(TMessage message)
        {
            return Task.CompletedTask;
        }
    }
}
