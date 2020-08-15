namespace NanoMessageBus.Serializers.NativeJson
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Abstractions.Enums;
    using Abstractions.Interfaces;

    public class NativeJsonSerialization : ISerialization
    {
        public SerializationEngine Identification => SerializationEngine.NativeJson;

        public async Task<byte[]> SerializeMessageAsync(IMessage message)
        {
            var stream = new MemoryStream();
            await System.Text.Json.JsonSerializer.SerializeAsync(stream, message, message.GetType());
            return stream.ToArray();
        }

        public async Task<object> DeserializeMessageAsync(byte[] array, Type receivedMessageType)
        {
            return await System.Text.Json.JsonSerializer.DeserializeAsync(new MemoryStream(array), receivedMessageType);
        }
    }
}
