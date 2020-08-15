namespace NanoMessageBus.Sender.EventArgs
{
    using System;
    using Abstractions.Interfaces;

    public class MessageSentEventArgs : EventArgs
    {
        public IMessage Message { get; set; }

        public Type MessageType { get; set; }

        public int MessageSize { get; set; }
    }
}
