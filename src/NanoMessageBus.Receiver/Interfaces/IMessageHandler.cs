namespace NanoMessageBus.Receiver.Interfaces
{
    using System.Threading.Tasks;
    using Abstractions.Interfaces;

    public interface IMessageHandler { }

    public interface IMessageHandler<in TMessage> : IMessageHandler where TMessage : IMessage
    {
        Task RegisterStatistics(long prepareToSendAt, long sentAt, long receivedAt, long handledAt);

        Task<bool> BeforeHandle(TMessage message);

        Task Handle(TMessage message);

        Task AfterHandle(TMessage message);
    }
}
