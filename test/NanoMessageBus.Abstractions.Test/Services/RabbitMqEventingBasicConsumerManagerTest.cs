namespace NanoMessageBus.Abstractions.Test.Services
{
    using Abstractions.Services;
    using Moq;
    using RabbitMQ.Client;
    using Xunit;

    public class RabbitMqEventingBasicConsumerManagerTest
    {
        [Fact]
        public void GetNewEventingBasicConsumer()
        {
            // arrange
            var channel = new Mock<IModel>();
            var manager = new RabbitMqEventingBasicConsumerManager();

            // act
            var consumer = manager.GetNewEventingBasicConsumer(channel.Object);

            // assert
            Assert.Equal(channel.Object, consumer.Model);
        }
    }
}
