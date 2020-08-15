namespace NanoMessageBus.Abstractions.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Enums;

    public interface ISerialization
    {
        public SerializationEngine Identification { get; }

        public Task<byte[]> SerializeMessageAsync(IMessage message);

        public Task<object> DeserializeMessageAsync(byte[] array, Type receivedMessageType);
    }
}
