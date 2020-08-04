namespace NanoMessageBus.Sender.Test
{
    using System;
    using System.Collections.Generic;
    using Abstractions.Interfaces;
    using DateTimeUtils.Interfaces;
    using Moq;
    using PropertyRetriever.Interfaces;
    using RabbitMQ.Client;
    using Services;
    using Xunit;

    public class SenderBusTest
    {
        private Mock<ILoggerFacade<SenderBus>> LoggerFacadeMock { get; }
        private Mock<IRabbitMqConnectionFactoryManager> ConnectionFactoryManagerMock { get; }
        private Mock<IDateTimeUtils> DateTimeUtilsMock { get; }
        private Mock<IPropertyRetriever> PropertyRetrieverMock { get; }

        private Mock<IConnectionFactory> ConnectionFactoryMock { get; }
        private Mock<IConnection> ConnectionMock { get; }
        private Mock<IModel> ChannelMock { get; }

        public SenderBusTest()
        {
            LoggerFacadeMock = new Mock<ILoggerFacade<SenderBus>>();
            ConnectionFactoryManagerMock = new Mock<IRabbitMqConnectionFactoryManager>();
            DateTimeUtilsMock = new Mock<IDateTimeUtils>();
            PropertyRetrieverMock = new Mock<IPropertyRetriever>();

            ConnectionFactoryMock = new Mock<IConnectionFactory>();
            ConnectionMock = new Mock<IConnection>();
            ChannelMock = new Mock<IModel>();
        }

        [Fact]
        public void SenderBus_Constructor()
        {
            // arrange
            const string identification = "identification";
            const int maxShardingSize = 10;
            const string rabbitHostNames = "server1:5872,server2:5872";
            const string rabbitVirtualHost = "virtualHost";
            const string rabbitUsername = "rabbitUserName";
            const string rabbitPassword = "rabbitPassword";

            #region PropertyRetriever
            PropertyRetrieverMock
                   .Setup(x => x.RetrieveFromCommandLineOrEnvironment("brokerIdentification", "brokerIdentification"))
                   .Returns(identification);

            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromEnvironment("brokerMaxShardingSize", It.IsAny<int>()))
                .Returns(maxShardingSize);

            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromCommandLineOrEnvironment("brokerHostname", "brokerHostname", It.IsAny<string>()))
                .Returns(rabbitHostNames);

            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromCommandLineOrEnvironment("brokerVirtualHost", "brokerVirtualHost", It.IsAny<string>()))
                .Returns(rabbitVirtualHost);

            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromCommandLineOrEnvironment("brokerUsername", "brokerUsername", It.IsAny<string>()))
                .Returns(rabbitUsername);

            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromCommandLineOrEnvironment("brokerPassword", "brokerPassword", It.IsAny<string>()))
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
            var senderBus = new SenderBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object);

            // assert
            Assert.Equal(ConnectionMock.Object, senderBus.Connection);
            Assert.Equal(maxShardingSize, senderBus.MaxShardingSize);
            Assert.Equal(identification, senderBus.Identification);
            Assert.Equal(DateTimeUtilsMock.Object, senderBus.DateTimeUtils);

            ConnectionFactoryManagerMock.Verify(x => x.GetConnectionFactory(rabbitUsername, rabbitVirtualHost, rabbitPassword, true), Times.Once);

            LoggerFacadeMock.Verify(x => x.LogDebug($"Sending with ServiceIdentification: {identification}"), Times.Once);
            LoggerFacadeMock.Verify(x => x.LogDebug($"Sending with MaxShardingSize: {maxShardingSize}"), Times.Once);
            LoggerFacadeMock.Verify(x => x.LogDebug($"Connecting to servers: {rabbitHostNames}"));

            for (var i = 0; i < maxShardingSize; i++)
            {
                ChannelMock.Verify(x => x.ExchangeDeclare($"exchange.{identification}.{i}", ExchangeType.Fanout, true, false, null), Times.Once);
                LoggerFacadeMock.Verify(x => x.LogDebug($"Creating fanout exchange exchange.{identification}.{i} to send messages."), Times.Once);
            }

            ChannelMock.Verify(x => x.Close(), Times.Once);
        }

        [Fact]
        public void SenderBus_Constructor_Error()
        {
            // arrange
            var ex = new Exception();
            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromEnvironment(It.IsAny<string>()))
                .Throws(ex);
            
            // act
            var result = Record.Exception(() => new SenderBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object));

            // assert
            Assert.IsType<InvalidOperationException>(result);
            Assert.Equal("A error has occurred, impossible to continue. Please see the inner exception for details.", result.Message);
        }

        [Fact]
        public void SendAsync()
        {
            // arrange

            // act

            // assert
        }
    }
}
