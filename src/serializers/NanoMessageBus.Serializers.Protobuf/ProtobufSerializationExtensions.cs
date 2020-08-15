namespace NanoMessageBus.Serializers.Protobuf
{
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    public static class ProtobufSerializationExtensions
    {
        public static IServiceCollection AddNanoMessageBusProtobufSerialization(this IServiceCollection @this)
        {
            @this.AddSingleton<ISerialization, ProtobufSerialization>();
            return @this;
        }
    }
}
