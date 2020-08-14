namespace NanoMessageBus.Serializers.Protobuf
{
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class ProtobufSerializationExtensions
    {
        public static IServiceCollection AddNanoMessageBusProtobufSerialization(this IServiceCollection @this)
        {
            @this.TryAddSingleton<ISerialization, ProtobufSerialization>();
            return @this;
        }
    }
}
