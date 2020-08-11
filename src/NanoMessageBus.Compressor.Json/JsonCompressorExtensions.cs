namespace NanoMessageBus.Compressor.Json
{
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class JsonCompressorExtensions
    {
        public static IServiceCollection AddNanoMessageBusJsonCompressor(this IServiceCollection @this)
        {
            @this.TryAddSingleton<ICompressor, JsonCompressor>();
            return @this;
        }
    }
}
