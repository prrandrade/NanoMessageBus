namespace NanoMessageBus.Sender
{
    using Abstractions.Enums;
    using Abstractions.Interfaces;
    using Abstractions.Services;
    using DateTimeUtils;
    using Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using PropertyRetriever;
    using Serializers.NativeJson;
    using Services;

    public static class NanoMessageBusSenderBusExtensions
    {
        public static IServiceCollection AddSenderBus(this IServiceCollection @this)
        {
            @this.AddPropertyRetriever();
            @this.AddDateTimeUtils();
            @this.AddNanoMessageBusNativeJsonSerializer();
            @this.AddLogging(c => c.AddConsole());
            @this.TryAddScoped(typeof(ILoggerFacade<>), typeof(LoggerFacade<>));
            @this.TryAddSingleton<IRabbitMqConnectionFactoryManager, RabbitMqConnectionFactoryManager>();
            @this.TryAddSingleton<ISenderBus, SenderBus>();
            return @this;
        }

        public static ServiceProvider UseSenderBus(this ServiceProvider @this, SerializationEngine defaultSerializationEngine = SerializationEngine.NativeJson)
        {
            var _ = @this.GetService<ISenderBus>();
            _.SetDefaultSerializationEngine(defaultSerializationEngine);
            return @this;
        }

        public static ISenderBus GetSenderBus(this ServiceProvider @this)
        {
            var bus = @this.GetService<ISenderBus>();
            return bus;
        }
    }
}
