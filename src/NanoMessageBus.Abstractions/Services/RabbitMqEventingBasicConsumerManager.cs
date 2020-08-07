namespace NanoMessageBus.Abstractions.Services
{
    using Interfaces;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    public class RabbitMqEventingBasicConsumerManager : IRabbitMqEventingBasicConsumerManager
    {
        public EventingBasicConsumer GetNewEventingBasicConsumer(IModel channel)
        {
            var consumer = new EventingBasicConsumer(channel);
            return consumer;
        }
    }
}
