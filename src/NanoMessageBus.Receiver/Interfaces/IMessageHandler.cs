namespace NanoMessageBus.Receiver.Interfaces
{
    using System.Threading.Tasks;
    using Abstractions;

    public interface IMessageHandler { }

    public interface IMessageHandler<in TMessage> : IMessageHandler where TMessage : IMessage
    {
        Task<bool> BeforeHandle(TMessage message);

        Task Handle(TMessage message);

        Task AfterHandle(TMessage message);
    }
}
