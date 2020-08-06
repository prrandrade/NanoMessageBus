namespace NanoMessageBus.Receiver.Test
{
    using System;
    using System.Collections.Generic;
    using Abstractions.Interfaces;
    using DateTimeUtils.Interfaces;
    using Handlers;
    using Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using PropertyRetriever.Interfaces;
    using RabbitMQ.Client;
    using Services;
    using Xunit;
    using static Abstractions.LocalConstants;

    public class ReceiverBusTest
    {
        private Mock<ILoggerFacade<ReceiverBus>> LoggerFacadeMock { get; }
        private Mock<IRabbitMqConnectionFactoryManager> ConnectionFactoryManagerMock { get; }
        private Mock<IServiceScopeFactory> ServiceScopeFactoryMock { get; }
        private Mock<IDateTimeUtils> DateTimeUtilsMock { get; }
        private Mock<IPropertyRetriever> PropertyRetrieverMock { get; }

        private Mock<IConnectionFactory> ConnectionFactoryMock { get; }
        private Mock<IConnection> ConnectionMock { get; }
        private Mock<IModel> ChannelMock { get; }
        
        private IEnumerable<IMessageHandler> Handlers { get; } = new List<IMessageHandler> { new DummyGuidHandler(), new DummyIntHandler() };

        public ReceiverBusTest()
        {
            LoggerFacadeMock = new Mock<ILoggerFacade<ReceiverBus>>();
            ConnectionFactoryManagerMock = new Mock<IRabbitMqConnectionFactoryManager>();
            ServiceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            DateTimeUtilsMock = new Mock<IDateTimeUtils>();
            PropertyRetrieverMock = new Mock<IPropertyRetriever>();

            ConnectionFactoryMock = new Mock<IConnectionFactory>();
            ConnectionMock = new Mock<IConnection>();
            ChannelMock = new Mock<IModel>();
        }

        #region Constructor

        [Theory]
        [InlineData(10, "0-9")]
        [InlineData(0, "0")]
        public void ReceiverBus_Constructor(int maxShardingSize, string listenedShards)
        {
            // arrange
            const string identification = "identification";
            const string listenedServices = "service1,service2,service3";
            const string rabbitHostNames = "server1:5872,server2:5872";
            const string rabbitVirtualHost = "virtualHost";
            const string rabbitUsername = "rabbitUserName";
            const string rabbitPassword = "rabbitPassword";
            const ushort prefetch = 50;

            #region PropertyRetriever
            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromCommandLineOrEnvironment(BrokerIdentificationProperty, BrokerIdentificationProperty, BrokerIdentificationFallbackValue))
                .Returns(identification);

            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromCommandLineOrEnvironment(BrokerPrefetchSizeProperty, BrokerPrefetchSizeProperty, It.IsAny<ushort>()))
                .Returns(prefetch);

            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromEnvironment(BrokerMaxShardingSizeProperty,  BrokerMaxShardingSizeFallbackValue))
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

            // act
            var receiverBus = new ReceiverBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, ServiceScopeFactoryMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object, Handlers);

            // assert
            if (maxShardingSize < 0)
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
            foreach (var queue in receiverBus.Queues)
                LoggerFacadeMock.Verify(x => x.LogDebug($"Preparing to consume queue {queue}."));
        }

        #endregion
    }
}
