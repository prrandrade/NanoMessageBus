namespace NanoMessageBus.Serializers.MessagePack
{
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    public static class MessagePackSerializationExtensions
    {
        public static IServiceCollection AddNanoMessageBusMessagePackSerialization(this IServiceCollection @this)
        {
            @this.AddSingleton<ISerialization, MessagePackSerialization>();
            return @this;
        }
    }
}
