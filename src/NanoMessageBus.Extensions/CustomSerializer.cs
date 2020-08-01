namespace NanoMessageBus.Extensions
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Abstractions;

    public static class CustomSerializer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<byte[]> CompressMessageAsync(IMessage message)
        {
            var stream = new MemoryStream();
            await System.Text.Json.JsonSerializer.SerializeAsync(stream, message, message.GetType());
            return stream.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<object> DecompressMessageAsync(Type receivedMessageType, byte[] array)
        {
            return await System.Text.Json.JsonSerializer.DeserializeAsync(new MemoryStream(array), receivedMessageType);
        }
    }
}
