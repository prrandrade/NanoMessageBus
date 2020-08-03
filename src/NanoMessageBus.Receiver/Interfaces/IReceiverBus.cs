namespace NanoMessageBus.Receiver.Interfaces
{
    using System;
    using Abstractions;

    public interface IReceiverBus
    {
        event EventHandler<MessageEventArgs> MessageReceived;
        event EventHandler<MessageEventArgs> MessageProcessed;

        void StartConsumer();
    }
}
