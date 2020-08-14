namespace NanoMessageBus.Compressor.MessagePack
{
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class MessagePackCompressorExtensions
    {
        public static IServiceCollection AddNanoMessageBusMessagePackCompressor(this IServiceCollection @this)
        {
            @this.TryAddSingleton<ICompressor, MessagePackCompressor>();
            return @this;
        }
    }
}
