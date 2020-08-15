namespace NanoMessageBus.Serializers.DeflateJson
{
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    public static class DeflateJsonSerializationExtensions
    {
        public static IServiceCollection AddNanoMessageBusDeflateJsonSerialization(this IServiceCollection @this)
        {
            @this.AddSingleton<ISerialization, DeflateJsonSerialization>();
            return @this;
        }
    }
}
