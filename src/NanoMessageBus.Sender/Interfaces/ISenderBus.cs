namespace NanoMessageBus.Sender.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Abstractions.Enums;
    using Abstractions.Interfaces;

    public interface ISenderBus
    {
        Task SendAsync(IMessage message, MessagePriority priority = MessagePriority.NormalPriority, Func<object, int, int> shardResolver = null);
    }
}
