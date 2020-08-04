namespace NanoMessageBus.Sender.Test
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Abstractions.Interfaces;
    using Abstractions.Services;
    using Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Moq;
    using RabbitMQ.Client;
    using Xunit;

    public class NanoMessageBusSenderBusExtensionsTest
    {
        [Fact]
        public void AddSenderBus()
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
            services.AddSenderBus();
            var container = services.BuildServiceProvider();
            var senderBus1 = container.GetService<ISenderBus>();
            var senderBus2 = container.GetService<ISenderBus>();

            // assert
            Assert.NotNull(container.GetService<ISenderBus>());
            Assert.Equal(senderBus1, senderBus2);
        }

        [Fact]
        public void LoadSenderBus()
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
            services.AddSenderBus();
            var container = services.BuildServiceProvider();
            
            // act
            container.LoadSenderBus();

            // assert
            mockConnectionFactoryManager.Verify(x => x.GetConnectionFactory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
            mockConnectionFactory.Verify(x => x.CreateConnection(It.IsAny<IList<string>>()), Times.Once);
            mockConnection.Verify(x => x.CreateModel(), Times.Once);
        }

        [Fact]
        public void GetSenderBus()
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
            services.AddSenderBus();
            var container = services.BuildServiceProvider();
            
            // act
            var bus = container.GetSenderBus();

            // assert
            mockConnectionFactoryManager.Verify(x => x.GetConnectionFactory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
            mockConnectionFactory.Verify(x => x.CreateConnection(It.IsAny<IList<string>>()), Times.Once);
            mockConnection.Verify(x => x.CreateModel(), Times.Once);
            Assert.Equal(container.GetService<ISenderBus>(), bus);
        }
    }
}
