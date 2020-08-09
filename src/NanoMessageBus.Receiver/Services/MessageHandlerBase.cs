namespace NanoMessageBus.Receiver.Services
{
    using System;
    using System.Threading.Tasks;
    using Abstractions.Interfaces;
    using Interfaces;

    public abstract class MessageHandlerBase<TMessage> : IMessageHandler<TMessage> where TMessage : IMessage
    {
        public virtual Task RegisterStatisticsAsync(DateTime prepareToSendAt, DateTime sentAt, DateTime receivedAt, DateTime handledAt)
        {
            return Task.CompletedTask;
        }

        public virtual Task<bool> BeforeHandleAsync(TMessage message)
        {
            return Task.FromResult(true);
        }

        public virtual Task HandleAsync(TMessage message)
        {
            return Task.CompletedTask;
        }

        public virtual Task AfterHandleAsync(TMessage message)
        {
            return Task.CompletedTask;
        }
    }
}
