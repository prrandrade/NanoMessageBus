namespace NanoMessageBus.Compressor.JsonCompressed
{
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class Extensions
    {
        public static IServiceCollection AddNanoMessageBusJsonCompressedCompressor(this IServiceCollection @this)
        {
            @this.TryAddSingleton<ICompressor, JsonCompressedCompressor>();
            return @this;
        }
    }
}
