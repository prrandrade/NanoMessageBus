namespace NanoMessageBus.Serializers.MessagePack
{
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class MessagePackSerializationExtensions
    {
        public static IServiceCollection AddNanoMessageBusMessagePackSerialization(this IServiceCollection @this)
        {
            @this.TryAddSingleton<ISerialization, MessagePackSerialization>();
            return @this;
        }
    }
}
