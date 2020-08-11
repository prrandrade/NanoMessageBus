namespace NanoMessageBus.Compressor.Protobuf
{
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class ProtobufCompressorExtensions
    {
        public static IServiceCollection AddNanoMessageBusProtobufCompressor(this IServiceCollection @this)
        {
            @this.TryAddSingleton<ICompressor, ProtobufCompressor>();
            return @this;
        }
    }
}
