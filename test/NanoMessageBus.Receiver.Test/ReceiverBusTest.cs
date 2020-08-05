namespace NanoMessageBus.Receiver.Test
{
    using System.Collections.Generic;
    using Abstractions.Interfaces;
    using DateTimeUtils.Interfaces;
    using Handlers;
    using Interfaces;
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
        private Mock<IDateTimeUtils> DateTimeUtilsMock { get; }
        private Mock<IPropertyRetriever> PropertyRetrieverMock { get; }

        private Mock<IConnectionFactory> ConnectionFactoryMock { get; }
        private Mock<IConnection> ConnectionMock { get; }
        private Mock<IModel> ChannelMock { get; }
        private Mock<IModel> SecondChannelMock { get; }


        private IEnumerable<IMessageHandler> Handlers { get; } = new List<IMessageHandler> { new DummyGuidHandler(), new DummyIntHandler() };

        public ReceiverBusTest()
        {
            LoggerFacadeMock = new Mock<ILoggerFacade<ReceiverBus>>();
            ConnectionFactoryManagerMock = new Mock<IRabbitMqConnectionFactoryManager>();
            DateTimeUtilsMock = new Mock<IDateTimeUtils>();
            PropertyRetrieverMock = new Mock<IPropertyRetriever>();

            ConnectionFactoryMock = new Mock<IConnectionFactory>();
            ConnectionMock = new Mock<IConnection>();
            ChannelMock = new Mock<IModel>();
            SecondChannelMock = new Mock<IModel>();
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

            #region PropertyRetriever
            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromCommandLineOrEnvironment(BrokerIdentificationProperty, BrokerIdentificationProperty, BrokerIdentificationFallbackValue))
                .Returns(identification);

            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromEnvironment(BrokerMaxShardingSizeProperty,  BrokerMaxShardingSizeFallbackValue))
                .Returns(maxShardingSize);

            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromCommandLineOrEnvironment(BrokerListenedServicesProperty, BrokerListenedServicesProperty, identification))
                .Returns(listenedServices);

            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromCommandLineOrEnvironment(BrokerListenedShardsProperty, BrokerListenedShardsProperty, $"0-{maxShardingSize-1}"))
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
        }

        #endregion
    }
}
