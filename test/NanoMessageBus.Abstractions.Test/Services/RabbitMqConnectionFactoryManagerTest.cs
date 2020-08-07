namespace NanoMessageBus.Abstractions.Test.Services
{
    using Abstractions.Services;
    using RabbitMQ.Client;
    using Xunit;

    public class RabbitMqConnectionFactoryManagerTest
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetConnectionFactory(bool automaticRecovery)
        {
            // arrange
            var manager = new RabbitMqConnectionFactoryManager();
            const string username = "username";
            const string virtualhost = "virtualhost";
            const string password = "password";
            
            // act
            var connectionFactory = (ConnectionFactory) manager.GetConnectionFactory(username, virtualhost, password, automaticRecovery);

            // assert
            Assert.Equal(username, connectionFactory.UserName);
            Assert.Equal(virtualhost, connectionFactory.VirtualHost);
            Assert.Equal(password, connectionFactory.Password);
            Assert.Equal(automaticRecovery, connectionFactory.AutomaticRecoveryEnabled);
        }
    }
}
