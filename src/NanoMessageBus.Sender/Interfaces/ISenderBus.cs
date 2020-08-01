namespace NanoMessageBus.Sender.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Abstractions;

    public interface ISenderBus
    {
        Task SendAsync(IMessage message, MessagePriority priority = MessagePriority.NormalPriority, Func<object, int, int> shardResolver = null);
    }
}
