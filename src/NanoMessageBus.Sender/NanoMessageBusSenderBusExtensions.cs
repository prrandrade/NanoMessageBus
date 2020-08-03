namespace NanoMessageBus.Sender
{
    using Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using PropertyRetriever;
    using Services;

    public static class NanoMessageBusSenderBusExtensions
    {
        public static IServiceCollection AddSenderBus(this IServiceCollection @this)
        {
            @this.AddPropertyRetriever();
            @this.AddLogging();
            @this.TryAddSingleton<ISenderBus, SenderBus>();
            return @this;
        }

        public static ISenderBus GetSenderBus(this ServiceProvider @this)
        {
            var bus = @this.GetService<ISenderBus>();
            return bus;
        }
    }
}
