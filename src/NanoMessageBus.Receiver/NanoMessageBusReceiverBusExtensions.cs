namespace NanoMessageBus.Receiver
{
    using System;
    using System.Linq;
    using Abstractions.Interfaces;
    using Abstractions.Services;
    using Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using PropertyRetriever;
    using Services;

    public static class NanoMessageBusReceiverBusExtensions
    {
        public static IServiceCollection AddReceiverBus(this IServiceCollection @this)
        {
            @this.AddPropertyRetriever();
            @this.AddLogging(c => c.AddConsole(x => x.IncludeScopes = false));
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var mytype in assembly.GetTypes().Where(mytype => mytype.GetInterfaces().Contains(typeof(IMessageHandler)))) 
                {
                    if (mytype.IsInterface || mytype.IsAbstract) continue;
                    @this.AddScoped(typeof(IMessageHandler), mytype);
                    @this.AddScoped(mytype);
                }
            }
            @this.TryAddSingleton<IRabbitMqConnectionFactoryManager, RabbitMqConnectionFactoryManager>();
            @this.AddSingleton<IReceiverBus, ReceiverBus>();
            return @this;
        }

        public static ServiceProvider LoadReceiverBus(this ServiceProvider @this)
        {
            @this.GetService<IReceiverBus>();
            return @this;
        }

        public static void ConsumeMessages(this ServiceProvider @this)
        {
            var bus = @this.GetService<IReceiverBus>();
            bus.StartConsumer();
        }
    }
}
