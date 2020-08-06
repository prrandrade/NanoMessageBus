namespace NanoMessageBus.Abstractions.Interfaces
{
    using System;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    public interface IRabbitMqEventingBasicConsumerManager
    {
        public EventingBasicConsumer GetNewEventingBasicConsumer(IModel channel);
    }
}
