namespace NanoMessageBus.Serializers.MessagePack
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Abstractions.Enums;
    using Abstractions.Interfaces;
    using global::MessagePack;
    using global::MessagePack.Resolvers;

    public class MessagePackSerialization : ISerialization
    {
        public SerializationEngine Identification => SerializationEngine.MessagePack;

        public async Task<byte[]> SerializeMessageAsync(IMessage message)
        {
            var stream = new MemoryStream();
            await global::MessagePack.MessagePackSerializer.SerializeAsync(message.GetType(), stream, message, MessagePackSerializerOptions.Standard
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

        public async Task<object> DeserializeMessageAsync(byte[] array, Type receivedMessageType)
        {
            return await global::MessagePack.MessagePackSerializer.DeserializeAsync(receivedMessageType, new MemoryStream(array), MessagePackSerializerOptions.Standard
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
