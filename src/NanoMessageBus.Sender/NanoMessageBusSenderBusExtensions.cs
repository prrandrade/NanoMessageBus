namespace NanoMessageBus.Sender
{
    using Abstractions.Interfaces;
    using Abstractions.Services;
    using DateTimeUtils;
    using Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using PropertyRetriever;
    using Services;

    public static class NanoMessageBusSenderBusExtensions
    {
        public static IServiceCollection AddSenderBus(this IServiceCollection @this)
        {
            @this.AddPropertyRetriever();
            @this.AddDateTimeUtils();
            @this.AddLogging(c => c.AddConsole(x => x.IncludeScopes = false));
            @this.TryAddScoped(typeof(ILoggerFacade<>), typeof(LoggerFacade<>));
            @this.TryAddSingleton<IRabbitMqConnectionFactoryManager, RabbitMqConnectionFactoryManager>();
            @this.TryAddSingleton<ISenderBus, SenderBus>();
            return @this;
        }

        public static ServiceProvider UseSenderBus(this ServiceProvider @this)
        {
            var _ = @this.GetService<ISenderBus>();
            return @this;
        }

        public static ISenderBus GetSenderBus(this ServiceProvider @this)
        {
            var bus = @this.GetService<ISenderBus>();
            return bus;
        }
    }
}
