namespace NanoMessageBus.Abstractions
{
    using System;

    public class MessageEventArgs : EventArgs
    {
        public IMessage Message { get; }
        public Type MessageType { get; }
        public DateTime SendStart { get; }
        public DateTime SendFinish { get; }
        public DateTime ReceiveStart { get; }
        public DateTime ReceiveFinish { get; }

        public MessageEventArgs(IMessage message, Type messageType, DateTime sendStart,
            DateTime sendFinish, DateTime receiveStart, DateTime receiveFinish)
        {
            Message = message;
            MessageType = messageType;
            SendStart = sendStart;
            SendFinish = sendFinish;
            ReceiveStart = receiveStart;
            ReceiveFinish = receiveFinish;
        }
    }
}
