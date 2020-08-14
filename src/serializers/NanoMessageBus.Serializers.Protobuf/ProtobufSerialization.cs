namespace NanoMessageBus.Serializers.Protobuf
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Abstractions.Interfaces;
    using ProtoBuf;

    public class ProtobufSerialization : ISerialization
    {
        public string Identification => "Protobuf";

        public async Task<byte[]> SerializeMessageAsync(IMessage message)
        {
            await Task.CompletedTask;
            var stream = new MemoryStream();
            Serializer.Serialize(stream, message);
            return stream.ToArray();
        }

        public async Task<object> DeserializeMessageAsync(byte[] array, Type receivedMessageType)
        {
            await Task.CompletedTask;
            var obj = Serializer.Deserialize(receivedMessageType, new MemoryStream(array));
            return obj;
        }
    }
}
