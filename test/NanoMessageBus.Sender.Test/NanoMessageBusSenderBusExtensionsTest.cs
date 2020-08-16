namespace NanoMessageBus.Sender.Test
{
    using System.Collections.Generic;
    using Abstractions.Enums;
    using Abstractions.Interfaces;
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

        [Theory]
        [InlineData(SerializationEngine.DeflateJson)]
        [InlineData(SerializationEngine.NativeJson)]
        [InlineData(SerializationEngine.MessagePack)]
        [InlineData(SerializationEngine.Protobuf)]
        public void UseSenderBus(SerializationEngine serializationEngine)
        {
            // arrange
            var deflateJsonSerialization = new Mock<ISerialization>();
            var nativeJsonSerialization = new Mock<ISerialization>();
            var messagePackSerialization = new Mock<ISerialization>();
            var protobufSerialization = new Mock<ISerialization>();

            deflateJsonSerialization.SetupGet(x => x.Identification).Returns(SerializationEngine.DeflateJson);
            nativeJsonSerialization.SetupGet(x => x.Identification).Returns(SerializationEngine.NativeJson);
            messagePackSerialization.SetupGet(x => x.Identification).Returns(SerializationEngine.MessagePack);
            protobufSerialization.SetupGet(x => x.Identification).Returns(SerializationEngine.Protobuf);

            var mockConnectionFactoryManager = new Mock<IRabbitMqConnectionFactoryManager>();
            var mockConnectionFactory = new Mock<IConnectionFactory>();
            var mockConnection = new Mock<IConnection>();
            var mockChannel = new Mock<IModel>();
            mockConnectionFactoryManager.Setup(x => x.GetConnectionFactory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(mockConnectionFactory.Object);
            mockConnectionFactory.Setup(x => x.CreateConnection(It.IsAny<IList<string>>())).Returns(mockConnection.Object);
            mockConnection.Setup(x => x.CreateModel()).Returns(mockChannel.Object);

            var services = new ServiceCollection();
            services.AddSingleton(deflateJsonSerialization.Object);
            services.AddSingleton(nativeJsonSerialization.Object);
            services.AddSingleton(messagePackSerialization.Object);
            services.AddSingleton(protobufSerialization.Object);

            services.TryAddSingleton(mockConnectionFactoryManager.Object);
            services.AddSenderBus();
            var container = services.BuildServiceProvider();
            
            // act
            container.UseSenderBus(serializationEngine);

            // assert
            Assert.Equal(serializationEngine, container.GetService<ISenderBus>().DefaultSerializationEngine.Identification);
            mockConnectionFactoryManager.Verify(x => x.GetConnectionFactory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
            mockConnectionFactory.Verify(x => x.CreateConnection(It.IsAny<IList<string>>()), Times.Once);
            mockConnection.Verify(x => x.CreateModel(), Times.Once);
        }

        [Fact]
        public void UseSenderBus_NoSerializationEngine()
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
            container.UseSenderBus();

            // assert
            Assert.Equal(SerializationEngine.NativeJson, container.GetService<ISenderBus>().DefaultSerializationEngine.Identification);
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
