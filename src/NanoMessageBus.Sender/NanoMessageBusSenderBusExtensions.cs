namespace NanoMessageBus.Sender
{
    using System.Threading;
    using Abstractions.Interfaces;
    using Abstractions.Services;
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
            @this.AddLogging(c => c.AddConsole(x => x.IncludeScopes = false));
            @this.TryAddSingleton<IRabbitMqConnectionFactoryManager, RabbitMqConnectionFactoryManager>();
            @this.TryAddSingleton<ISenderBus, SenderBus>();
            return @this;
        }

        public static ServiceProvider LoadSenderBus(this ServiceProvider @this)
        {
            @this.GetService<ISenderBus>();
            Thread.Sleep(100);
            return @this;
        }

        public static ISenderBus GetSenderBus(this ServiceProvider @this)
        {
            var bus = @this.GetService<ISenderBus>();
            return bus;
        }
    }
}
