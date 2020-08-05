namespace NanoMessageBus.Receiver.Interfaces
{
    using System.Threading.Tasks;
    using Abstractions.Interfaces;

    public interface IMessageHandler { }

    public interface IMessageHandler<in TMessage> : IMessageHandler where TMessage : IMessage
    {
        Task RegisterStatisticsAsync(long prepareToSendAt, long sentAt, long receivedAt, long handledAt);

        Task<bool> BeforeHandleAsync(TMessage message);

        Task HandleAsync(TMessage message);

        Task AfterHandleAsync(TMessage message);
    }
}
