namespace NanoMessageBus.Receiver.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Threading.Channels;
    using Abstractions.Interfaces;
    using DateTimeUtils.Interfaces;
    using Handlers;
    using Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Models;
    using Moq;
    using PropertyRetriever.Interfaces;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using Services;
    using Xunit;
    using static Abstractions.LocalConstants;

    public class ReceiverBusTest
    {
        private Mock<ILoggerFacade<ReceiverBus>> LoggerFacadeMock { get; }
        private Mock<IRabbitMqConnectionFactoryManager> ConnectionFactoryManagerMock { get; }
        private Mock<IRabbitMqEventingBasicConsumerManager> BasicConsumerManagerMock { get; }

        private Mock<IServiceScopeFactory> ServiceScopeFactoryMock { get; }
        private Mock<IServiceScope> ServiceScopeMock { get; }
        private Mock<IServiceProvider> ServiceProviderMock { get; }

        private Mock<IDateTimeUtils> DateTimeUtilsMock { get; }
        private Mock<IPropertyRetriever> PropertyRetrieverMock { get; }
        private Mock<ICompressor> CompressorMock { get; }

        private Mock<IConnectionFactory> ConnectionFactoryMock { get; }
        private Mock<IConnection> ConnectionMock { get; }
        private Mock<IModel> ChannelMock { get; }

        private IEnumerable<IMessageHandler> Handlers { get; } = new List<IMessageHandler> { new DummyGuidHandler(), new DummyIntHandler() };

        public ReceiverBusTest()
        {
            LoggerFacadeMock = new Mock<ILoggerFacade<ReceiverBus>>();
            ConnectionFactoryManagerMock = new Mock<IRabbitMqConnectionFactoryManager>();
            BasicConsumerManagerMock = new Mock<IRabbitMqEventingBasicConsumerManager>();

            ServiceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            ServiceScopeMock = new Mock<IServiceScope>();
            ServiceProviderMock = new Mock<IServiceProvider>();
            CompressorMock = new Mock<ICompressor>();

            DateTimeUtilsMock = new Mock<IDateTimeUtils>();
            PropertyRetrieverMock = new Mock<IPropertyRetriever>();
            ConnectionFactoryMock = new Mock<IConnectionFactory>();
            ConnectionMock = new Mock<IConnection>();
            ChannelMock = new Mock<IModel>();
        }

        #region Constructor

        [Theory]
        [InlineData("exampleService", 10, "0-9", 50, false)]
        [InlineData("exampleService", 0, "0", 10, true)]
        public void ReceiverBus_Constructor(string identification, int maxShardingSize, string listenedShards, ushort prefetch, bool autoAck)
        {
            // arrange
            PrepareForReceiverBus(identification, maxShardingSize, listenedShards, prefetch, autoAck);

            // act
            var receiverBus = new ReceiverBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, BasicConsumerManagerMock.Object, ServiceScopeFactoryMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object, CompressorMock.Object, Handlers);

            // assert
            if (maxShardingSize <= 0)
                LoggerFacadeMock.Verify(x => x.LogWarning($"Property {BrokerIdentificationProperty} is invalid, will be treated as 1!"), Times.Once);

            foreach (var listenedShard in receiverBus.ListenedShards)
            {
                ChannelMock.Verify(x => x.QueueDeclare($"queue.{identification}.{listenedShard}", true, false, false, null), Times.Once);
                Assert.Contains($"queue.{identification}.{listenedShard}", receiverBus.Queues);
                foreach (var listenedService in receiverBus.ListenedServices)
                {
                    ChannelMock.Verify(x => x.ExchangeDeclare($"exchange.{listenedService}.{listenedShard}", ExchangeType.Fanout, true, false, null), Times.Once);
                    ChannelMock.Verify(x => x.QueueBind($"queue.{identification}.{listenedShard}", $"exchange.{listenedService}.{listenedShard}", string.Empty, null), Times.Once);
                    LoggerFacadeMock.Verify(x => x.LogDebug($"Binding Exchange exchange.{listenedService}.{listenedShard} with Queue queue.{identification}.{listenedShard}."));
                }
            }

            ChannelMock.Verify(x => x.BasicQos(0, prefetch, true), Times.Exactly(receiverBus.Queues.Count));
            BasicConsumerManagerMock.Verify(x => x.GetNewEventingBasicConsumer(ChannelMock.Object), Times.Exactly(receiverBus.Queues.Count));

            foreach (var queue in receiverBus.Queues)
            {
                Assert.NotNull(receiverBus.Consumers[queue]);
                var eventInfo = typeof(EventingBasicConsumer).GetField(nameof(EventingBasicConsumer.Received), BindingFlags.Instance | BindingFlags.NonPublic);
                var list = (eventInfo.GetValue(receiverBus.Consumers[queue]) as MulticastDelegate).GetInvocationList();
                Assert.Single(list);
                LoggerFacadeMock.Verify(x => x.LogDebug($"Preparing to consume queue {queue}."));
            }
        }

        [Fact]
        public void ReceiverBus_Constructor_Error()
        {
            // arrange
            var ex = new Exception();
            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromCommandLineOrEnvironment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(ex);

            // act
            var result = Record.Exception(() => new ReceiverBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, BasicConsumerManagerMock.Object, ServiceScopeFactoryMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object, CompressorMock.Object, Handlers));

            // assert
            Assert.IsType<InvalidOperationException>(result);
            Assert.Equal("A error has occurred, impossible to continue. Please see the inner exception for details.", result.Message);
            Assert.Equal(ex, result.InnerException);
        }

        #endregion

        #region StartConsumer

        [Theory]
        [InlineData("exampleService", 10, "0-9", 50, true)]
        public void StartConsumer(string identification, int maxShardingSize, string listenedShards, ushort prefetch, bool autoAck)
        {
            // arrange
            PrepareForReceiverBus(identification, maxShardingSize, listenedShards, prefetch, autoAck);
            var receiverBus = new ReceiverBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, BasicConsumerManagerMock.Object, ServiceScopeFactoryMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object, CompressorMock.Object, Handlers);

            // act
            receiverBus.StartConsumer();

            // assert
            foreach (var queue in receiverBus.Queues)
            {
                ChannelMock.Verify(x => x.BasicConsume(queue, autoAck, It.IsAny<string>(), false, false, null, receiverBus.Consumers[queue]), Times.Once);
            }
        }

        #endregion

        #region ConsumeMessageAsync

        [Theory]
        [InlineData("exampleService", 10, "0-9", 50, true)]
        public async void ConsumeMessageAsync_IntMessageId(string identification, int maxShardingSize, string listenedShards, ushort prefetch, bool autoAck)
        {
            // arrange
            PrepareForReceiverBus(identification, maxShardingSize, listenedShards, prefetch, autoAck);

            var prepareToSendAt = new DateTime(2002, 1, 1);
            var sentAt = new DateTime(2003, 1, 1);
            var receivedAt = new DateTime(2004, 1, 1);
            var handledAt = new DateTime(2005, 1, 1);

            DateTimeUtilsMock
                .SetupSequence(x => x.UtcNow())
                .Returns(receivedAt)
                .Returns(handledAt);

            var receiverBus = new ReceiverBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, BasicConsumerManagerMock.Object, ServiceScopeFactoryMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object, CompressorMock.Object, Handlers);

            var message = new DummyIntMessage();
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, message, message.GetType());
            var byteContent = stream.ToArray();
            CompressorMock
                .Setup(x => x.DecompressMessageAsync(It.IsAny<byte[]>(), It.IsAny<Type>()))
                .ReturnsAsync(message);

            var ea = new BasicDeliverEventArgs
            {
                Body = byteContent,
                DeliveryTag = ulong.MaxValue,
                BasicProperties = new DummyBasicProperties
                {
                    Type = message.GetType().AssemblyQualifiedName,
                    Headers = new Dictionary<string, object>
                    {
                        {"prepareToSendAt", prepareToSendAt.ToBinary()},
                        {"sentAt", sentAt.ToBinary()}
                    }
                }
            };

            // act
            await receiverBus.ConsumeMessageAsync(ChannelMock.Object, ea);

            // assert
            var handler = (DummyIntHandler) Handlers.First(x => x.GetType() == typeof(DummyIntHandler));
            Assert.True(handler.RegisterStatisticsAsyncPassed);
            Assert.True(handler.BeforeHandlerAsyncPassed);
            Assert.True(handler.HandleAsyncPassed);
            Assert.True(handler.AfterHandleAsyncPassed);

            if (!autoAck)
                ChannelMock.Verify(x => x.BasicAck(ea.DeliveryTag, false), Times.Once);
        }

        [Theory]
        [InlineData("exampleService", 0, "0", 10, true)]
        public async void ConsumeMessageAsync_GuidMessageId(string identification, int maxShardingSize, string listenedShards, ushort prefetch, bool autoAck)
        {
            // arrange
            PrepareForReceiverBus(identification, maxShardingSize, listenedShards, prefetch, autoAck);

            var prepareToSendAt = new DateTime(2002, 1, 1);
            var sentAt = new DateTime(2003, 1, 1);
            var receivedAt = new DateTime(2004, 1, 1);
            var handledAt = new DateTime(2005, 1, 1);

            DateTimeUtilsMock
                .SetupSequence(x => x.UtcNow())
                .Returns(receivedAt)
                .Returns(handledAt);

            var receiverBus = new ReceiverBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, BasicConsumerManagerMock.Object, ServiceScopeFactoryMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object, CompressorMock.Object, Handlers);

            var message = new DummyGuidMessage();
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, message, message.GetType());
            var byteContent = stream.ToArray();
            CompressorMock
                .Setup(x => x.DecompressMessageAsync(It.IsAny<byte[]>(), It.IsAny<Type>()))
                .ReturnsAsync(message);

            var ea = new BasicDeliverEventArgs
            {
                Body = byteContent,
                DeliveryTag = ulong.MaxValue,
                BasicProperties = new DummyBasicProperties
                {
                    Type = message.GetType().AssemblyQualifiedName,
                    Headers = new Dictionary<string, object>
                    {
                        {"prepareToSendAt", prepareToSendAt.ToBinary()},
                        {"sentAt", sentAt.ToBinary()}
                    }
                }
            };

            // act
            await receiverBus.ConsumeMessageAsync(ChannelMock.Object, ea);

            // assert
            var handler = (DummyGuidHandler) Handlers.First(x => x.GetType() == typeof(DummyGuidHandler));
            Assert.True(handler.RegisterStatisticsAsyncPassed);
            Assert.True(handler.BeforeHandlerAsyncPassed);
            Assert.True(handler.HandleAsyncPassed);
            Assert.True(handler.AfterHandleAsyncPassed);

            if (!autoAck)
                ChannelMock.Verify(x => x.BasicAck(ea.DeliveryTag, false), Times.Once);
        }

        [Theory]
        [InlineData("exampleService", 0, "0", 10, true)]
        public async void ConsumeMessageAsync_UnrecognizableType(string identification, int maxShardingSize, string listenedShards, ushort prefetch, bool autoAck)
        {
            // arrange
            PrepareForReceiverBus(identification, maxShardingSize, listenedShards, prefetch, autoAck);

            var prepareToSendAt = new DateTime(2002, 1, 1);
            var sentAt = new DateTime(2003, 1, 1);
            var receivedAt = new DateTime(2004, 1, 1);
            var handledAt = new DateTime(2005, 1, 1);

            DateTimeUtilsMock
                .SetupSequence(x => x.UtcNow())
                .Returns(receivedAt)
                .Returns(handledAt);

            var receiverBus = new ReceiverBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, BasicConsumerManagerMock.Object, ServiceScopeFactoryMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object, CompressorMock.Object, Handlers);

            var message = new DummyGuidMessage();
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, message, message.GetType());
            var byteContent = stream.ToArray();
            var ea = new BasicDeliverEventArgs
            {
                Body = byteContent,
                DeliveryTag = ulong.MaxValue,
                BasicProperties = new DummyBasicProperties
                {
                    Type = "unrecognizable type!",
                    Headers = new Dictionary<string, object>
                    {
                        {"prepareToSendAt", prepareToSendAt.ToBinary()},
                        {"sentAt", sentAt.ToBinary()}
                    }
                }
            };

            // act
            await receiverBus.ConsumeMessageAsync(ChannelMock.Object, ea);

            // assert
            LoggerFacadeMock.Verify(x => x.LogWarning($"Unrecognizable type {ea.BasicProperties.Type} for delivered message!"));

            if (!autoAck)
                ChannelMock.Verify(x => x.BasicAck(ea.DeliveryTag, false), Times.Once);
        }

        [Theory]
        [InlineData("exampleService", 0, "0", 10, true)]
        public async void ConsumeMessageAsync_UnHandlebleType(string identification, int maxShardingSize, string listenedShards, ushort prefetch, bool autoAck)
        {
            // arrange
            PrepareForReceiverBus(identification, maxShardingSize, listenedShards, prefetch, autoAck);

            var prepareToSendAt = new DateTime(2002, 1, 1);
            var sentAt = new DateTime(2003, 1, 1);
            var receivedAt = new DateTime(2004, 1, 1);
            var handledAt = new DateTime(2005, 1, 1);

            DateTimeUtilsMock
                .SetupSequence(x => x.UtcNow())
                .Returns(receivedAt)
                .Returns(handledAt);

            var receiverBus = new ReceiverBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, BasicConsumerManagerMock.Object, ServiceScopeFactoryMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object, CompressorMock.Object, Handlers);

            var message = new DummyGuidMessage();
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, message, message.GetType());
            var byteContent = stream.ToArray();
            var ea = new BasicDeliverEventArgs
            {
                Body = byteContent,
                DeliveryTag = ulong.MaxValue,
                BasicProperties = new DummyBasicProperties
                {
                    Type = typeof(int).ToString(),
                    Headers = new Dictionary<string, object>
                    {
                        {"prepareToSendAt", prepareToSendAt.ToBinary()},
                        {"sentAt", sentAt.ToBinary()}
                    }
                }
            };

            // act
            await receiverBus.ConsumeMessageAsync(ChannelMock.Object, ea);

            // assert
            LoggerFacadeMock.Verify(x => x.LogWarning($"There's no handler for {ea.BasicProperties.Type}. This message will be ignored!"));

            if (!autoAck)
                ChannelMock.Verify(x => x.BasicAck(ea.DeliveryTag, false), Times.Once);
        }

        #endregion

        #region Consuming Messages via event 

        [Theory]
        [InlineData("exampleService", 10, "0-9", 50, true)]
        public async void ConsumeMessageAsync_IntMessageId_ViaEvent(string identification, int maxShardingSize, string listenedShards, ushort prefetch, bool autoAck)
        {
            // arrange
            PrepareForReceiverBus(identification, maxShardingSize, listenedShards, prefetch, autoAck);

            var prepareToSendAt = new DateTime(2002, 1, 1);
            var sentAt = new DateTime(2003, 1, 1);
            var receivedAt = new DateTime(2004, 1, 1);
            var handledAt = new DateTime(2005, 1, 1);

            DateTimeUtilsMock
                .SetupSequence(x => x.UtcNow())
                .Returns(receivedAt)
                .Returns(handledAt);

            var receiverBus = new ReceiverBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, BasicConsumerManagerMock.Object, ServiceScopeFactoryMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object, CompressorMock.Object, Handlers);

            var message = new DummyIntMessage { Id = 0 };
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, message, message.GetType());
            var byteContent = stream.ToArray();
            CompressorMock
                .Setup(x => x.DecompressMessageAsync(It.IsAny<byte[]>(), It.IsAny<Type>()))
                .ReturnsAsync(message);

            var ea = new BasicDeliverEventArgs
            {
                Body = byteContent,
                DeliveryTag = ulong.MaxValue,
                BasicProperties = new DummyBasicProperties
                {
                    Type = message.GetType().AssemblyQualifiedName,
                    Headers = new Dictionary<string, object>
                    {
                        {"prepareToSendAt", prepareToSendAt.ToBinary()},
                        {"sentAt", sentAt.ToBinary()}
                    }
                }
            };

            // act
            var queue = $"queue.{identification}.{message.Id}";
            receiverBus.Consumers[queue].HandleBasicDeliver(Guid.NewGuid().ToString(), ea.DeliveryTag, false, string.Empty, string.Empty, ea.BasicProperties, byteContent);

            // assert
            var handler = (DummyIntHandler) Handlers.First(x => x.GetType() == typeof(DummyIntHandler));
            Assert.True(handler.RegisterStatisticsAsyncPassed);
            Assert.True(handler.BeforeHandlerAsyncPassed);
            Assert.True(handler.HandleAsyncPassed);
            Assert.True(handler.AfterHandleAsyncPassed);

            if (!autoAck)
                ChannelMock.Verify(x => x.BasicAck(ea.DeliveryTag, false), Times.Once);
        }

        #endregion

        private void PrepareForReceiverBus(string identification, int maxShardingSize, string listenedShards, ushort prefetch, bool autoAck)
        {
            // arrange
            const string listenedServices = "service1,service2,service3";
            const string rabbitHostNames = "server1:5872,server2:5872";
            const string rabbitVirtualHost = "virtualHost";
            const string rabbitUsername = "rabbitUserName";
            const string rabbitPassword = "rabbitPassword";

            #region PropertyRetriever

            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromCommandLineOrEnvironment(BrokerIdentificationProperty, BrokerIdentificationProperty, BrokerIdentificationFallbackValue))
                .Returns(identification);

            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromCommandLineOrEnvironment(BrokerPrefetchSizeProperty, BrokerPrefetchSizeProperty, It.IsAny<ushort>()))
                .Returns(prefetch);

            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromEnvironment(BrokerMaxShardingSizeProperty, BrokerMaxShardingSizeFallbackValue))
                .Returns(maxShardingSize);

            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromCommandLineOrEnvironment(BrokerListenedServicesProperty, BrokerListenedServicesProperty, identification))
                .Returns(listenedServices);

            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromCommandLineOrEnvironment(BrokerListenedShardsProperty, BrokerListenedShardsProperty, It.IsAny<string>()))
                .Returns(listenedShards);

            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromCommandLineOrEnvironment(BrokerHostnameProperty, BrokerHostnameProperty, BrokerHostnameFallbackValue))
                .Returns(rabbitHostNames);

            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromCommandLineOrEnvironment(BrokerVirtualHostProperty, BrokerVirtualHostProperty, BrokerVirtualHostFallbackValue))
                .Returns(rabbitVirtualHost);

            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromCommandLineOrEnvironment(BrokerUsernameProperty, BrokerUsernameProperty, BrokerUsernameFallbackValue))
                .Returns(rabbitUsername);

            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromCommandLineOrEnvironment(BrokerPasswordProperty, BrokerPasswordProperty, BrokerPasswordFallbackValue))
                .Returns(rabbitPassword);

            PropertyRetrieverMock
                .Setup(x => x.CheckFromCommandLine(BrokerAutoAckProperty))
                .Returns(autoAck);

            #endregion

            #region Connection Factory Manager

            ConnectionFactoryManagerMock
                .Setup(x => x.GetConnectionFactory(rabbitUsername, rabbitVirtualHost, rabbitPassword, true))
                .Returns(ConnectionFactoryMock.Object);

            ConnectionFactoryMock
                .Setup(x => x.CreateConnection(It.IsAny<IList<string>>()))
                .Returns(ConnectionMock.Object);

            ConnectionMock
                .Setup(x => x.CreateModel())
                .Returns(ChannelMock.Object);

            #endregion

            #region Basic Consumer

            BasicConsumerManagerMock
                .Setup(x => x.GetNewEventingBasicConsumer(ChannelMock.Object))
                .Returns(() => new EventingBasicConsumer(ChannelMock.Object));

            #endregion

            #region Service Scope Factory

            ServiceScopeFactoryMock
                .Setup(x => x.CreateScope())
                .Returns(ServiceScopeMock.Object);

            ServiceScopeMock
                .SetupGet(x => x.ServiceProvider)
                .Returns(ServiceProviderMock.Object);

            ServiceProviderMock
                .Setup(x => x.GetService(typeof(DummyIntHandler)))
                .Returns(Handlers.First(x => x.GetType() == typeof(DummyIntHandler)));

            ServiceProviderMock
                .Setup(x => x.GetService(typeof(DummyGuidHandler)))
                .Returns(Handlers.First(x => x.GetType() == typeof(DummyGuidHandler)));

            #endregion
        }
    }
}
