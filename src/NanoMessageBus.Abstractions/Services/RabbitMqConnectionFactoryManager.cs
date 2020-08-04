namespace NanoMessageBus.Abstractions.Services
{
    using Interfaces;
    using RabbitMQ.Client;

    public class RabbitMqConnectionFactoryManager : IRabbitMqConnectionFactoryManager
    {
        public IConnectionFactory GetConnectionFactory(string userName, string virtualHost, string password, bool automaticRecoveryEnabled)
        {
            return new ConnectionFactory
            {
                UserName = userName,
                VirtualHost = virtualHost,
                Password = password,
                AutomaticRecoveryEnabled = automaticRecoveryEnabled
            };
        }
    }
}
