namespace NanoMessageBus.Compressor.MessagePack
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Abstractions.Interfaces;
    using global::MessagePack;
    using global::MessagePack.Resolvers;

    public class MessagePackCompressor : ICompressor
    {
        public string Identification => "Message Pack";

        public async Task<byte[]> CompressMessageAsync(IMessage message)
        {
            var stream = new MemoryStream();
            await MessagePackSerializer.SerializeAsync(message.GetType(), stream, message, MessagePackSerializerOptions.Standard
                .WithResolver(CompositeResolver.Create(
                    NativeDateTimeResolver.Instance,
                    NativeGuidResolver.Instance,
                    NativeDecimalResolver.Instance,
                    TypelessObjectResolver.Instance,
                    ContractlessStandardResolver.Instance,
                    StandardResolver.Instance,
                    DynamicContractlessObjectResolver.Instance
                )));
            return stream.ToArray();
        }

        public async Task<object> DecompressMessageAsync(byte[] array, Type receivedMessageType)
        {
            return await MessagePackSerializer.DeserializeAsync(receivedMessageType, new MemoryStream(array), MessagePackSerializerOptions.Standard
                .WithResolver(CompositeResolver.Create(
                    NativeDateTimeResolver.Instance,
                    NativeGuidResolver.Instance,
                    NativeDecimalResolver.Instance,
                    TypelessObjectResolver.Instance,
                    ContractlessStandardResolver.Instance,
                    StandardResolver.Instance,
                    DynamicContractlessObjectResolver.Instance
                )));
        }
    }
}
