namespace NanoMessageBus.Receiver.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Abstractions.Interfaces;

    public interface IMessageHandler { }

    public interface IMessageHandler<in TMessage> : IMessageHandler where TMessage : IMessage
    {
        Task RegisterStatisticsAsync(DateTime prepareToSendAt, DateTime sentAt, DateTime receivedAt, DateTime handledAt);

        Task<bool> BeforeHandleAsync(TMessage message);

        Task HandleAsync(TMessage message);

        Task AfterHandleAsync(TMessage message);
    }
}
