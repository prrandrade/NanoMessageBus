namespace NanoMessageBus.Serializers.DeflateJson
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.CompilerServices;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Abstractions.Interfaces;

    public class DeflateJsonSerialization : ISerialization
    {
        public string Identification => "Deflate Json";

        public async Task<byte[]> SerializeMessageAsync(IMessage message)
        {
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, message, message.GetType());
            return CompressJson(stream.ToArray());
        }

        public async Task<object> DeserializeMessageAsync(byte[] array, Type receivedMessageType)
        {
            return await JsonSerializer.DeserializeAsync(new MemoryStream(DecompressJson(array)), receivedMessageType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte[] CompressJson(byte[] data)
        {
            var output = new MemoryStream();
            using (var dstream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte[] DecompressJson(byte[] data)
        {
            var input = new MemoryStream(data);
            var output = new MemoryStream();
            using (var dstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }
    }
}
