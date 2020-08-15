namespace NanoMessageBus.Serializers.NativeJson
{
    using System.Linq;
    using Abstractions.Enums;
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    public static class NativeJsonSerializationExtensions
    {
        public static IServiceCollection AddNanoMessageBusNativeJsonSerializer(this IServiceCollection @this)
        {
            if (!@this.Any(x => x.ImplementationType == typeof(NativeJsonSerialization) && x.ServiceType == typeof(ISerialization)))
                @this.AddSingleton<ISerialization, NativeJsonSerialization>();
            return @this;
        }
    }
}
