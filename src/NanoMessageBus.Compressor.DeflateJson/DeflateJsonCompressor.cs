namespace NanoMessageBus.Compressor.DeflateJson
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.CompilerServices;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Abstractions.Interfaces;

    public class DeflateJsonCompressor : ICompressor
    {
        public string Identification => "Deflate Json";

        public async Task<byte[]> CompressMessageAsync(IMessage message)
        {
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, message, message.GetType());
            return JsonCompress(stream.ToArray());
        }

        public async Task<object> DecompressMessageAsync(byte[] array, Type receivedMessageType)
        {
            return await JsonSerializer.DeserializeAsync(new MemoryStream(JsonDecompress(array)), receivedMessageType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte[] JsonCompress(byte[] data)
        {
            var output = new MemoryStream();
            using (var dstream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte[] JsonDecompress(byte[] data)
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
