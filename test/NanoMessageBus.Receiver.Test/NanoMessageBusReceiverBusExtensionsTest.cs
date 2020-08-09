namespace NanoMessageBus.Receiver.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions.Interfaces;
    using Handlers;
    using Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Moq;
    using RabbitMQ.Client;
    using Xunit;

    public class NanoMessageBusReceiverBusExtensionsTest
    {
        [Fact]
        public void AddReceiverBus()
        {
            // arrange
            var mockConnectionFactoryManager = new Mock<IRabbitMqConnectionFactoryManager>();
            var mockConnectionFactory = new Mock<IConnectionFactory>();
            var mockConnection = new Mock<IConnection>();
            var mockChannel = new Mock<IModel>();
            mockConnectionFactoryManager.Setup(x => x.GetConnectionFactory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(mockConnectionFactory.Object);
            mockConnectionFactory.Setup(x => x.CreateConnection(It.IsAny<IList<string>>())).Returns(mockConnection.Object);
            mockConnection.Setup(x => x.CreateModel()).Returns(mockChannel.Object);
            var services = new ServiceCollection();
            services.TryAddSingleton(mockConnectionFactoryManager.Object);

            // act
            services.AddReceiverBus();
            var container = services.BuildServiceProvider();
            var receiverBus1 = container.GetService<IReceiverBus>();
            var receiverBus2 = container.GetService<IReceiverBus>();

            // assert
            Assert.NotNull(container.GetService<IReceiverBus>());
            Assert.Equal(receiverBus1, receiverBus2);

            var handlers = container.GetServices<IMessageHandler>().ToList();
            Assert.Equal(1, handlers.Count(x => x.GetType() == typeof(DummyGuidHandler)));
            Assert.Equal(1, handlers.Count(x => x.GetType() == typeof(DummyIntHandler)));
        }

        [Fact]
        public void UseReceiverBus()
        {
            // arrange
            var mockConnectionFactoryManager = new Mock<IRabbitMqConnectionFactoryManager>();
            var mockConnectionFactory = new Mock<IConnectionFactory>();
            var mockConnection = new Mock<IConnection>();
            var mockChannel = new Mock<IModel>();
            mockConnectionFactoryManager.Setup(x => x.GetConnectionFactory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(mockConnectionFactory.Object);
            mockConnectionFactory.Setup(x => x.CreateConnection(It.IsAny<IList<string>>())).Returns(mockConnection.Object);
            mockConnection.Setup(x => x.CreateModel()).Returns(mockChannel.Object);

            var services = new ServiceCollection();
            services.TryAddSingleton(mockConnectionFactoryManager.Object);
            services.AddReceiverBus();
            var container = services.BuildServiceProvider();

            // act
            container.UseReceiverBus();

            // assert
            mockConnectionFactoryManager.Verify(x => x.GetConnectionFactory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
            mockConnectionFactory.Verify(x => x.CreateConnection(It.IsAny<IList<string>>()), Times.Once);
            mockConnection.Verify(x => x.CreateModel(), Times.AtLeastOnce);
        }

        [Fact]
        public void ConsumeMessages()
        {
            // arrange
            var mockConnectionFactoryManager = new Mock<IRabbitMqConnectionFactoryManager>();
            var mockConnectionFactory = new Mock<IConnectionFactory>();
            var mockConnection = new Mock<IConnection>();
            var mockChannel = new Mock<IModel>();
            mockConnectionFactoryManager.Setup(x => x.GetConnectionFactory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(mockConnectionFactory.Object);
            mockConnectionFactory.Setup(x => x.CreateConnection(It.IsAny<IList<string>>())).Returns(mockConnection.Object);
            mockConnection.Setup(x => x.CreateModel()).Returns(mockChannel.Object);

            var services = new ServiceCollection();
            services.TryAddSingleton(mockConnectionFactoryManager.Object);
            services.AddReceiverBus();
            var container = services.BuildServiceProvider();

            // act
            container.ConsumeMessages();

            // assert
            mockConnectionFactoryManager.Verify(x => x.GetConnectionFactory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
            mockConnectionFactory.Verify(x => x.CreateConnection(It.IsAny<IList<string>>()), Times.Once);
            mockConnection.Verify(x => x.CreateModel(), Times.AtLeastOnce);
            mockChannel.Verify(x => x.BasicConsume(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<IBasicConsumer>()), Times.Once);
        }
    }
}
