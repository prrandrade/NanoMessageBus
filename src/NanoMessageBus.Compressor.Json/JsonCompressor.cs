namespace NanoMessageBus.Compressor.Json
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Abstractions.Interfaces;

    public class JsonCompressor : ICompressor
    {
        public async Task<byte[]> CompressMessageAsync(IMessage message)
        {
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, message, message.GetType());
            return stream.ToArray();
        }

        public async Task<object> DecompressMessageAsync(byte[] array, Type receivedMessageType)
        {
            return await JsonSerializer.DeserializeAsync(new MemoryStream(array), receivedMessageType);
        }
    }
}
