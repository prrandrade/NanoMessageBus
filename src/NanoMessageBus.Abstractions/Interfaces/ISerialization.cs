namespace NanoMessageBus.Abstractions.Interfaces
{
    using System;
    using System.Threading.Tasks;

    public interface ISerialization
    {
        public string Identification { get; }

        public Task<byte[]> SerializeMessageAsync(IMessage message);

        public Task<object> DeserializeMessageAsync(byte[] array, Type receivedMessageType);
    }
}
