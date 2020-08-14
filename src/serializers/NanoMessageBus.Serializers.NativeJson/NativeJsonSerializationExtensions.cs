namespace NanoMessageBus.Serializers.NativeJson
{
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class NativeJsonSerializationExtensions
    {
        public static IServiceCollection AddNanoMessageBusNativeJsonSerializer(this IServiceCollection @this)
        {
            @this.TryAddSingleton<ISerialization, NativeJsonSerialization>();
            return @this;
        }
    }
}
