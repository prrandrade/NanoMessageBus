namespace NanoMessageBus.Compressor.Protobuf
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Abstractions.Interfaces;
    using ProtoBuf;

    public class ProtobufCompressor : ICompressor
    {
        public string Identification => "Protobuf";

        public async Task<byte[]> CompressMessageAsync(IMessage message)
        {
            await Task.CompletedTask;
            var stream = new MemoryStream();
            Serializer.Serialize(stream, message);
            return stream.ToArray();
        }

        public async Task<object> DecompressMessageAsync(byte[] array, Type receivedMessageType)
        {
            await Task.CompletedTask;
            var obj = Serializer.Deserialize(receivedMessageType, new MemoryStream(array));
            return obj;
        }
    }
}
