namespace NanoMessageBus.Abstractions.Interfaces
{
    using System;
    using System.Threading.Tasks;

    public interface ICompressor
    {
        public string Identification { get; }

        public Task<byte[]> CompressMessageAsync(IMessage message);

        public Task<object> DecompressMessageAsync(byte[] array, Type receivedMessageType);
    }
}
