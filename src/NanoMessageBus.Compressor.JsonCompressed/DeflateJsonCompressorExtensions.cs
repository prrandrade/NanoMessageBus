namespace NanoMessageBus.Compressor.DeflateJson
{
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class DeflateJsonCompressorExtensions
    {
        public static IServiceCollection AddNanoMessageBusDeflateJsonCompressor(this IServiceCollection @this)
        {
            @this.TryAddSingleton<ICompressor, DeflateJsonCompressor>();
            return @this;
        }
    }
}
