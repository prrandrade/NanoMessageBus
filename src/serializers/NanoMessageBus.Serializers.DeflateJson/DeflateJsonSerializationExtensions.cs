namespace NanoMessageBus.Serializers.DeflateJson
{
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class DeflateJsonSerializationExtensions
    {
        public static IServiceCollection AddNanoMessageBusDeflateJsonSerialization(this IServiceCollection @this)
        {
            @this.TryAddSingleton<ISerialization, DeflateJsonSerialization>();
            return @this;
        }
    }
}
