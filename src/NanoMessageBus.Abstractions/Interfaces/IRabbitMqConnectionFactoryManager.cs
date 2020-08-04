namespace NanoMessageBus.Abstractions.Interfaces
{
    using RabbitMQ.Client;

    public interface IRabbitMqConnectionFactoryManager
    {
        public IConnectionFactory GetConnectionFactory(string userName, string virtualHost, string password, bool automaticRecoveryEnabled);
    }
}
