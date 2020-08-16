namespace NanoMessageBus.Sender.Test
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Abstractions.Attributes;
    using Abstractions.Enums;
    using Abstractions.Interfaces;
    using DateTimeUtils.Interfaces;
    using EventArgs;
    using Models;
    using Moq;
    using PropertyRetriever.Interfaces;
    using RabbitMQ.Client;
    using Services;
    using Xunit;
    using static Abstractions.LocalConstants;

    public class SenderBusTest
    {
        private Mock<ILoggerFacade<SenderBus>> LoggerFacadeMock { get; }
        private Mock<IRabbitMqConnectionFactoryManager> ConnectionFactoryManagerMock { get; }
        private Mock<IDateTimeUtils> DateTimeUtilsMock { get; }
        private Mock<IPropertyRetriever> PropertyRetrieverMock { get; }
        private Mock<ISerialization> SerializerMockNativeJson { get; }
        private Mock<ISerialization> SerializerMockDeflateJson { get; }
        private IEnumerable<ISerialization> Serializers { get; }

        private Mock<IConnectionFactory> ConnectionFactoryMock { get; }
        private Mock<IConnection> ConnectionMock { get; }
        private Mock<IModel> ChannelMock { get; }
        private Mock<IModel> SecondChannelMock { get; }

        public SenderBusTest()
        {
            LoggerFacadeMock = new Mock<ILoggerFacade<SenderBus>>();
            ConnectionFactoryManagerMock = new Mock<IRabbitMqConnectionFactoryManager>();
            DateTimeUtilsMock = new Mock<IDateTimeUtils>();
            PropertyRetrieverMock = new Mock<IPropertyRetriever>();
            SerializerMockNativeJson = new Mock<ISerialization>();
            SerializerMockDeflateJson = new Mock<ISerialization>();

            SerializerMockNativeJson.SetupGet(x => x.Identification).Returns(SerializationEngine.NativeJson);
            SerializerMockDeflateJson.SetupGet(x => x.Identification).Returns(SerializationEngine.DeflateJson);

            Serializers = new List<ISerialization> { SerializerMockNativeJson.Object, SerializerMockDeflateJson.Object };

            ConnectionFactoryMock = new Mock<IConnectionFactory>();
            ConnectionMock = new Mock<IConnection>();
            ChannelMock = new Mock<IModel>();
            SecondChannelMock = new Mock<IModel>();
        }

        #region Constructor

        [Theory]
        [InlineData(10)]
        [InlineData(0)]
        public void SenderBus_Constructor(int maxShardingSize)
        {
            // arrange
            const string identification = "identification";
            const string rabbitHostNames = "server1:5872,server2:5872";
            const string rabbitVirtualHost = "virtualHost";
            const string rabbitUsername = "rabbitUserName";
            const string rabbitPassword = "rabbitPassword";

            #region PropertyRetriever
            PropertyRetrieverMock
                   .Setup(x => x.RetrieveFromCommandLineOrEnvironment(BrokerIdentificationProperty, BrokerIdentificationProperty, BrokerIdentificationFallbackValue))
                   .Returns(identification);

            PropertyRetrieverMock
                .Setup(x => x.RetrieveFromEnvironment(BrokerMaxShardingSizeProperty, BrokerMaxShardingSizeFallbackValue))
                .Returns(maxShardingSize);

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
            var senderBus = new SenderBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object, Serializers);

            // assert
            if (maxShardingSize < 0)
                LoggerFacadeMock.Verify(x => x.LogWarning($"Property {BrokerIdentificationProperty} is invalid, will be treated as 1!"), Times.Once);

            Assert.Equal(ConnectionMock.Object, senderBus.Connection);
            Assert.Equal(maxShardingSize > 0 ? maxShardingSize : 1, senderBus.MaxShardingSize);
            Assert.Equal(identification, senderBus.Identification);
            Assert.Equal(DateTimeUtilsMock.Object, senderBus.DateTimeUtils);
            Assert.Equal(SerializerMockNativeJson.Object, senderBus.DefaultSerializationEngine);

            ConnectionFactoryManagerMock.Verify(x => x.GetConnectionFactory(rabbitUsername, rabbitVirtualHost, rabbitPassword, true), Times.Once);

            LoggerFacadeMock.Verify(x => x.LogDebug($"Sending with ServiceIdentification: {identification}"), Times.Once);
            LoggerFacadeMock.Verify(x => x.LogDebug($"Sending with MaxShardingSize: {(maxShardingSize > 0 ? maxShardingSize : 1)}"), Times.Once);
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
            var result = Record.Exception(() => new SenderBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object, Serializers));

            // assert
            Assert.IsType<InvalidOperationException>(result);
            Assert.Equal("A error has occurred, impossible to continue. Please see the inner exception for details.", result.Message);
        }

        #endregion

        #region SetDefaultSerializationEngine

        [Theory]
        [InlineData(SerializationEngine.NativeJson, SerializationEngine.NativeJson)]
        [InlineData(SerializationEngine.DeflateJson, SerializationEngine.DeflateJson)]
        [InlineData(SerializationEngine.MessagePack, SerializationEngine.NativeJson)] // falling back
        [InlineData(SerializationEngine.Protobuf, SerializationEngine.NativeJson)] // falling back
        public void SetDefaultSerializationEngine(SerializationEngine choice, SerializationEngine expected)
        {
            // arrange - creating senderbus
            const string identification = "identification";
            const int maxShardingSize = 10;
            const string rabbitHostNames = "server1:5872,server2:5872";
            const string rabbitVirtualHost = "virtualHost";
            const string rabbitUsername = "rabbitUserName";
            const string rabbitPassword = "rabbitPassword";

            var prepareToSendAt = new DateTime(2002, 1, 1);
            var sentAt = new DateTime(2003, 1, 1);

            #region DateTimeUtils

            DateTimeUtilsMock
                .SetupSequence(x => x.UtcNow())
                .Returns(prepareToSendAt)
                .Returns(sentAt);

            #endregion

            #region PropertyRetriever

            PropertyRetrieverMock
                   .Setup(x => x.RetrieveFromCommandLineOrEnvironment("brokerIdentification", "brokerIdentification", It.IsAny<string>()))
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

            var basicProperties = new DummyBasicProperties();

            ConnectionFactoryManagerMock
                .Setup(x => x.GetConnectionFactory(rabbitUsername, rabbitVirtualHost, rabbitPassword, true))
                .Returns(ConnectionFactoryMock.Object);

            ConnectionFactoryMock
                .Setup(x => x.CreateConnection(It.IsAny<IList<string>>()))
                .Returns(ConnectionMock.Object);

            ConnectionMock
                .SetupSequence(x => x.CreateModel())
                .Returns(ChannelMock.Object)
                .Returns(SecondChannelMock.Object);

            SecondChannelMock
                .Setup(x => x.CreateBasicProperties())
                .Returns(basicProperties);

            #endregion

            var senderBus = new SenderBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object, Serializers);

            // act
            senderBus.SetDefaultSerializationEngine(choice);

            // assert
            Assert.Equal(expected, senderBus.DefaultSerializationEngine.Identification);
        }

        [Fact]
        public void SetDefaultSerializationEngine_Fallback()
        {
            // arrange - creating senderbus
            const string identification = "identification";
            const int maxShardingSize = 10;
            const string rabbitHostNames = "server1:5872,server2:5872";
            const string rabbitVirtualHost = "virtualHost";
            const string rabbitUsername = "rabbitUserName";
            const string rabbitPassword = "rabbitPassword";

            var prepareToSendAt = new DateTime(2002, 1, 1);
            var sentAt = new DateTime(2003, 1, 1);

            #region DateTimeUtils

            DateTimeUtilsMock
                .SetupSequence(x => x.UtcNow())
                .Returns(prepareToSendAt)
                .Returns(sentAt);

            #endregion

            #region PropertyRetriever

            PropertyRetrieverMock
                   .Setup(x => x.RetrieveFromCommandLineOrEnvironment("brokerIdentification", "brokerIdentification", It.IsAny<string>()))
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

            var basicProperties = new DummyBasicProperties();

            ConnectionFactoryManagerMock
                .Setup(x => x.GetConnectionFactory(rabbitUsername, rabbitVirtualHost, rabbitPassword, true))
                .Returns(ConnectionFactoryMock.Object);

            ConnectionFactoryMock
                .Setup(x => x.CreateConnection(It.IsAny<IList<string>>()))
                .Returns(ConnectionMock.Object);

            ConnectionMock
                .SetupSequence(x => x.CreateModel())
                .Returns(ChannelMock.Object)
                .Returns(SecondChannelMock.Object);

            SecondChannelMock
                .Setup(x => x.CreateBasicProperties())
                .Returns(basicProperties);

            #endregion

            var senderBus = new SenderBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object, Serializers);

            // act
            senderBus.SetDefaultSerializationEngine();

            // assert
            Assert.Equal(SerializationEngine.NativeJson, senderBus.DefaultSerializationEngine.Identification);
        }

        #endregion

        #region SendAsync

        [Fact]
        public async Task SendAsync_Default()
        {
            // arrange
            const string identification = "identification";
            const int maxShardingSize = 10;
            const string rabbitHostNames = "server1:5872,server2:5872";
            const string rabbitVirtualHost = "virtualHost";
            const string rabbitUsername = "rabbitUserName";
            const string rabbitPassword = "rabbitPassword";

            var prepareToSendAt = new DateTime(2002, 1, 1);
            var sentAt = new DateTime(2003, 1, 1);

            #region DateTimeUtils

            DateTimeUtilsMock
                .SetupSequence(x => x.UtcNow())
                .Returns(prepareToSendAt)
                .Returns(sentAt);

            #endregion

            #region PropertyRetriever
            PropertyRetrieverMock
                   .Setup(x => x.RetrieveFromCommandLineOrEnvironment("brokerIdentification", "brokerIdentification", It.IsAny<string>()))
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

            var basicProperties = new DummyBasicProperties();

            ConnectionFactoryManagerMock
                .Setup(x => x.GetConnectionFactory(rabbitUsername, rabbitVirtualHost, rabbitPassword, true))
                .Returns(ConnectionFactoryMock.Object);

            ConnectionFactoryMock
                .Setup(x => x.CreateConnection(It.IsAny<IList<string>>()))
                .Returns(ConnectionMock.Object);

            ConnectionMock
                .SetupSequence(x => x.CreateModel())
                .Returns(ChannelMock.Object)
                .Returns(SecondChannelMock.Object);

            SecondChannelMock
                .Setup(x => x.CreateBasicProperties())
                .Returns(basicProperties);

            #endregion

            var senderBus = new SenderBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object, Serializers);
            var message = new DummyIntMessage { Id = 0 };
            var expectedExchangeName = $"exchange.{identification}.0";
            var byteArray = Array.Empty<byte>();

            SerializerMockNativeJson
                .Setup(x => x.SerializeMessageAsync(message))
                .ReturnsAsync(byteArray);

            // act
            await senderBus.SendAsync(message);

            // assert
            Assert.Equal(prepareToSendAt.ToBinary(), basicProperties.Headers["prepareToSendAt"]);
            Assert.Equal(sentAt.ToBinary(), basicProperties.Headers["sentAt"]);
            Assert.Equal(2, basicProperties.DeliveryMode);
            Assert.Equal(typeof(DummyIntMessage).AssemblyQualifiedName, basicProperties.Type);
            Assert.True(basicProperties.Persistent);
            Assert.Equal(0, basicProperties.Priority);

            SecondChannelMock.Verify(x => x.CreateBasicProperties(), Times.Once);
            SecondChannelMock.Verify(x => x.BasicPublish(expectedExchangeName, string.Empty, false, basicProperties, byteArray), Times.Once);
            SecondChannelMock.Verify(x => x.Close(), Times.Once);
            SecondChannelMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Theory]
        [InlineData(MessagePriority.NormalPriority, 0)]
        [InlineData(MessagePriority.Level1Priority, 1)]
        [InlineData(MessagePriority.Level2Priority, 2)]
        [InlineData(MessagePriority.Level3Priority, 3)]
        [InlineData(MessagePriority.Level4Priority, 4)]
        public async Task SendAsync_DifferentPriorities(MessagePriority priority, byte expectedPriority)
        {
            // arrange
            const string identification = "identification";
            const int maxShardingSize = 10;
            const string rabbitHostNames = "server1:5872,server2:5872";
            const string rabbitVirtualHost = "virtualHost";
            const string rabbitUsername = "rabbitUserName";
            const string rabbitPassword = "rabbitPassword";

            var prepareToSendAt = new DateTime(2002, 1, 1);
            var sentAt = new DateTime(2003, 1, 1);

            #region DateTimeUtils

            DateTimeUtilsMock
                .SetupSequence(x => x.UtcNow())
                .Returns(prepareToSendAt)
                .Returns(sentAt);

            #endregion

            #region PropertyRetriever
            PropertyRetrieverMock
                   .Setup(x => x.RetrieveFromCommandLineOrEnvironment("brokerIdentification", "brokerIdentification", It.IsAny<string>()))
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

            var basicProperties = new DummyBasicProperties();

            ConnectionFactoryManagerMock
                .Setup(x => x.GetConnectionFactory(rabbitUsername, rabbitVirtualHost, rabbitPassword, true))
                .Returns(ConnectionFactoryMock.Object);

            ConnectionFactoryMock
                .Setup(x => x.CreateConnection(It.IsAny<IList<string>>()))
                .Returns(ConnectionMock.Object);

            ConnectionMock
                .SetupSequence(x => x.CreateModel())
                .Returns(ChannelMock.Object)
                .Returns(SecondChannelMock.Object);

            SecondChannelMock
                .Setup(x => x.CreateBasicProperties())
                .Returns(basicProperties);

            #endregion

            var senderBus = new SenderBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object, Serializers);
            var message = new DummyIntMessage { Id = 0 };
            var expectedExchangeName = $"exchange.{identification}.0";

            var byteArray = Array.Empty<byte>();

            SerializerMockNativeJson
                .Setup(x => x.SerializeMessageAsync(message))
                .ReturnsAsync(byteArray);


            // act
            await senderBus.SendAsync(message, priority);

            // assert
            Assert.Equal(prepareToSendAt.ToBinary(), basicProperties.Headers["prepareToSendAt"]);
            Assert.Equal(sentAt.ToBinary(), basicProperties.Headers["sentAt"]);
            Assert.Equal(2, basicProperties.DeliveryMode);
            Assert.Equal(typeof(DummyIntMessage).AssemblyQualifiedName, basicProperties.Type);
            Assert.True(basicProperties.Persistent);
            Assert.Equal(expectedPriority, basicProperties.Priority);

            SecondChannelMock.Verify(x => x.CreateBasicProperties(), Times.Once);
            SecondChannelMock.Verify(x => x.BasicPublish(expectedExchangeName, string.Empty, false, basicProperties, byteArray), Times.Once);
            SecondChannelMock.Verify(x => x.Close(), Times.Once);
            SecondChannelMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Theory]
        [InlineData(SerializationEngine.NativeJson, SerializationEngine.NativeJson)]
        [InlineData(SerializationEngine.DeflateJson, SerializationEngine.DeflateJson)]
        [InlineData(SerializationEngine.MessagePack, SerializationEngine.NativeJson)] // fallback
        [InlineData(SerializationEngine.Protobuf, SerializationEngine.NativeJson)] // fallback
        public async Task SendAsync_DifferentSerializationEngines(SerializationEngine serializationEngine, SerializationEngine expectedChoice)
        {
            // arrange
            const string identification = "identification";
            const int maxShardingSize = 10;
            const string rabbitHostNames = "server1:5872,server2:5872";
            const string rabbitVirtualHost = "virtualHost";
            const string rabbitUsername = "rabbitUserName";
            const string rabbitPassword = "rabbitPassword";

            var prepareToSendAt = new DateTime(2002, 1, 1);
            var sentAt = new DateTime(2003, 1, 1);

            #region DateTimeUtils

            DateTimeUtilsMock
                .SetupSequence(x => x.UtcNow())
                .Returns(prepareToSendAt)
                .Returns(sentAt);

            #endregion

            #region PropertyRetriever
            PropertyRetrieverMock
                   .Setup(x => x.RetrieveFromCommandLineOrEnvironment("brokerIdentification", "brokerIdentification", It.IsAny<string>()))
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

            var basicProperties = new DummyBasicProperties();

            ConnectionFactoryManagerMock
                .Setup(x => x.GetConnectionFactory(rabbitUsername, rabbitVirtualHost, rabbitPassword, true))
                .Returns(ConnectionFactoryMock.Object);

            ConnectionFactoryMock
                .Setup(x => x.CreateConnection(It.IsAny<IList<string>>()))
                .Returns(ConnectionMock.Object);

            ConnectionMock
                .SetupSequence(x => x.CreateModel())
                .Returns(ChannelMock.Object)
                .Returns(SecondChannelMock.Object);

            SecondChannelMock
                .Setup(x => x.CreateBasicProperties())
                .Returns(basicProperties);

            #endregion

            var senderBus = new SenderBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object, Serializers);
            var message = new DummyIntMessage { Id = 0 };
            var expectedExchangeName = $"exchange.{identification}.0";

            var byteArray = Array.Empty<byte>();

            SerializerMockNativeJson
                .Setup(x => x.SerializeMessageAsync(message))
                .ReturnsAsync(byteArray);

            SerializerMockDeflateJson
                .Setup(x => x.SerializeMessageAsync(message))
                .ReturnsAsync(byteArray);

            // act
            await senderBus.SendAsync(message, serializationEngine);

            // assert
            Assert.Equal(prepareToSendAt.ToBinary(), basicProperties.Headers["prepareToSendAt"]);
            Assert.Equal(sentAt.ToBinary(), basicProperties.Headers["sentAt"]);
            Assert.Equal(2, basicProperties.DeliveryMode);
            Assert.Equal(typeof(DummyIntMessage).AssemblyQualifiedName, basicProperties.Type);
            Assert.True(basicProperties.Persistent);
            Assert.Equal(expectedChoice, (SerializationEngine)basicProperties.Headers["serializer"]);

            SecondChannelMock.Verify(x => x.CreateBasicProperties(), Times.Once);
            SecondChannelMock.Verify(x => x.BasicPublish(expectedExchangeName, string.Empty, false, basicProperties, byteArray), Times.Once);
            SecondChannelMock.Verify(x => x.Close(), Times.Once);
            SecondChannelMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public async Task SendAsync_CustomizedShardResolver()
        {
            // arrange
            const string identification = "identification";
            const int maxShardingSize = 10;
            const string rabbitHostNames = "server1:5872,server2:5872";
            const string rabbitVirtualHost = "virtualHost";
            const string rabbitUsername = "rabbitUserName";
            const string rabbitPassword = "rabbitPassword";

            var prepareToSendAt = new DateTime(2002, 1, 1);
            var sentAt = new DateTime(2003, 1, 1);

            #region DateTimeUtils

            DateTimeUtilsMock
                .SetupSequence(x => x.UtcNow())
                .Returns(prepareToSendAt)
                .Returns(sentAt);

            #endregion

            #region PropertyRetriever
            PropertyRetrieverMock
                   .Setup(x => x.RetrieveFromCommandLineOrEnvironment("brokerIdentification", "brokerIdentification", It.IsAny<string>()))
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

            var basicProperties = new DummyBasicProperties();

            ConnectionFactoryManagerMock
                .Setup(x => x.GetConnectionFactory(rabbitUsername, rabbitVirtualHost, rabbitPassword, true))
                .Returns(ConnectionFactoryMock.Object);

            ConnectionFactoryMock
                .Setup(x => x.CreateConnection(It.IsAny<IList<string>>()))
                .Returns(ConnectionMock.Object);

            ConnectionMock
                .SetupSequence(x => x.CreateModel())
                .Returns(ChannelMock.Object)
                .Returns(SecondChannelMock.Object);

            SecondChannelMock
                .Setup(x => x.CreateBasicProperties())
                .Returns(basicProperties);

            #endregion

            var senderBus = new SenderBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object, Serializers);
            var message = new DummyIntMessage { Id = 5 };
            var expectedExchangeName = $"exchange.{identification}.0";
            static int CustomShardResolver(object o, int i) => 0;
            var byteArray = Array.Empty<byte>();

            SerializerMockNativeJson
                .Setup(x => x.SerializeMessageAsync(message))
                .ReturnsAsync(byteArray);

            // act
            await senderBus.SendAsync(message, CustomShardResolver);

            // assert
            Assert.Equal(prepareToSendAt.ToBinary(), basicProperties.Headers["prepareToSendAt"]);
            Assert.Equal(sentAt.ToBinary(), basicProperties.Headers["sentAt"]);
            Assert.Equal(2, basicProperties.DeliveryMode);
            Assert.Equal(typeof(DummyIntMessage).AssemblyQualifiedName, basicProperties.Type);
            Assert.True(basicProperties.Persistent);
            Assert.Equal(0, basicProperties.Priority);

            SecondChannelMock.Verify(x => x.CreateBasicProperties(), Times.Once);
            SecondChannelMock.Verify(x => x.BasicPublish(expectedExchangeName, string.Empty, false, basicProperties, byteArray), Times.Once);
            SecondChannelMock.Verify(x => x.Close(), Times.Once);
            SecondChannelMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public async Task SendAsync_CheckingEvent()
        {
            // arrange
            const string identification = "identification";
            const int maxShardingSize = 10;
            const string rabbitHostNames = "server1:5872,server2:5872";
            const string rabbitVirtualHost = "virtualHost";
            const string rabbitUsername = "rabbitUserName";
            const string rabbitPassword = "rabbitPassword";

            var prepareToSendAt = new DateTime(2002, 1, 1);
            var sentAt = new DateTime(2003, 1, 1);

            #region DateTimeUtils

            DateTimeUtilsMock
                .SetupSequence(x => x.UtcNow())
                .Returns(prepareToSendAt)
                .Returns(sentAt);

            #endregion

            #region PropertyRetriever
            PropertyRetrieverMock
                   .Setup(x => x.RetrieveFromCommandLineOrEnvironment("brokerIdentification", "brokerIdentification", It.IsAny<string>()))
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

            var basicProperties = new DummyBasicProperties();

            ConnectionFactoryManagerMock
                .Setup(x => x.GetConnectionFactory(rabbitUsername, rabbitVirtualHost, rabbitPassword, true))
                .Returns(ConnectionFactoryMock.Object);

            ConnectionFactoryMock
                .Setup(x => x.CreateConnection(It.IsAny<IList<string>>()))
                .Returns(ConnectionMock.Object);

            ConnectionMock
                .SetupSequence(x => x.CreateModel())
                .Returns(ChannelMock.Object)
                .Returns(SecondChannelMock.Object);

            SecondChannelMock
                .Setup(x => x.CreateBasicProperties())
                .Returns(basicProperties);

            #endregion

            var senderBus = new SenderBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object, Serializers);
            var message = new DummyIntMessage { Id = 0 };
            var expectedExchangeName = $"exchange.{identification}.0";
            var byteArray = Array.Empty<byte>();

            SerializerMockNativeJson
                .Setup(x => x.SerializeMessageAsync(message))
                .ReturnsAsync(byteArray);

            MessageSentEventArgs args = null;
            senderBus.MessageSent += (sender, a) => args = a;

            // act
            await senderBus.SendAsync(message);

            // assert
            Assert.Equal(prepareToSendAt.ToBinary(), basicProperties.Headers["prepareToSendAt"]);
            Assert.Equal(sentAt.ToBinary(), basicProperties.Headers["sentAt"]);
            Assert.Equal(2, basicProperties.DeliveryMode);
            Assert.Equal(typeof(DummyIntMessage).AssemblyQualifiedName, basicProperties.Type);
            Assert.True(basicProperties.Persistent);
            Assert.Equal(0, basicProperties.Priority);

            Assert.NotNull(args);
            Assert.Equal(message, args.Message);
            Assert.Equal(byteArray.Length, args.MessageSize);
            Assert.Equal(message.GetType(), args.MessageType);

            SecondChannelMock.Verify(x => x.CreateBasicProperties(), Times.Once);
            SecondChannelMock.Verify(x => x.BasicPublish(expectedExchangeName, string.Empty, false, basicProperties, byteArray), Times.Once);
            SecondChannelMock.Verify(x => x.Close(), Times.Once);
            SecondChannelMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public async Task SendAsync_Error()
        {
            // arrange
            const string identification = "identification";
            const int maxShardingSize = 10;
            const string rabbitHostNames = "server1:5872,server2:5872";
            const string rabbitVirtualHost = "virtualHost";
            const string rabbitUsername = "rabbitUserName";
            const string rabbitPassword = "rabbitPassword";

            var prepareToSendAt = new DateTime(2002, 1, 1);
            var sentAt = new DateTime(2003, 1, 1);
            var exception = new Exception("exception message");

            #region DateTimeUtils

            DateTimeUtilsMock
                .SetupSequence(x => x.UtcNow())
                .Returns(prepareToSendAt)
                .Returns(sentAt);

            #endregion

            #region PropertyRetriever
            PropertyRetrieverMock
                   .Setup(x => x.RetrieveFromCommandLineOrEnvironment("brokerIdentification", "brokerIdentification", It.IsAny<string>()))
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
                .SetupSequence(x => x.CreateModel())
                .Returns(ChannelMock.Object)
                .Throws(exception);

            #endregion

            var senderBus = new SenderBus(LoggerFacadeMock.Object, ConnectionFactoryManagerMock.Object, PropertyRetrieverMock.Object, DateTimeUtilsMock.Object, Serializers);
            var message = new DummyIntMessage { Id = 0 };

            // act
            var result = await Record.ExceptionAsync(async () => await senderBus.SendAsync(message));

            // assert
            Assert.IsType<InvalidOperationException>(result);
            Assert.Equal("A error has occurred, impossible to continue. Please see the inner exception for details.", result.Message);
            Assert.Equal(exception, result.InnerException);
        }

        #endregion

        #region GetMessageId

        [Fact]
        public void GetMessageId_Int()
        {
            // arrange
            var message = new DummyIntMessage { Id = int.MaxValue };

            // act
            var (returnedType, returnedObject) = SenderBus.GetMessageId(message);

            // assert
            Assert.Equal(typeof(int), returnedType);
            Assert.Equal(message.Id, returnedObject);
        }

        [Fact]
        public void GetMessageId_Guid()
        {
            // arrange
            var message = new DummyGuidMessage() { Id = Guid.NewGuid() };

            // act
            var (returnedType, returnedObject) = SenderBus.GetMessageId(message);

            // assert
            Assert.Equal(typeof(Guid), returnedType);
            Assert.Equal(message.Id, returnedObject);
        }

        [Fact]
        public void GetMessage_IncorrectMessageId()
        {
            // arrange
            var message = new DummyWrongIdMessage() { Id = double.Epsilon };

            // act
            var result = Record.Exception(() => SenderBus.GetMessageId(message));

            // assert
            Assert.IsType<ArgumentException>(result);
            Assert.Equal($"Incompatible type for property with {nameof(MessageIdAttribute)} property. Only {nameof(Int32)} or {nameof(Guid)} types are valid.", result.Message);
        }

        [Fact]
        public void GetMessage_NoMessageId()
        {
            // arrange
            var message = new DummyNoIdMessage();

            // act
            var result = Record.Exception(() => SenderBus.GetMessageId(message));

            // assert
            Assert.IsType<ArgumentException>(result);
            Assert.Equal($"No {nameof(MessageIdAttribute)} property was found!", result.Message);
        }

        #endregion

        #region GetShardResolver

        [Fact]
        public void GetShardResolver_NotNull()
        {
            // arrange
            static int Func(object o, int i) => i + 1;

            // act
            var result = SenderBus.GetShardResolver(typeof(object), Func);

            // assert
            Assert.Equal(Func, result);
        }

        [Fact]
        public void GetShardResolver_Int()
        {
            // act
            var result = SenderBus.GetShardResolver(typeof(int), null);

            // assert
            Assert.Equal(0, result(0, 10));
            Assert.Equal(1, result(1, 10));
            Assert.Equal(5, result(5, 10));
            Assert.Equal(0, result(10, 10));
            Assert.Equal(1, result(11, 10));
            Assert.Equal(5, result(5, 10));
        }

        [Fact]
        public void GetShardResolver_Guid()
        {
            // act
            var result = SenderBus.GetShardResolver(typeof(Guid), null);

            // assert
            Assert.Equal(0, result(Guid.Parse("00000000-0000-0000-0000-000000000000"), 10));
            Assert.Equal(1, result(Guid.Parse("01000000-0000-0000-0000-000000000000"), 10));
            Assert.Equal(5, result(Guid.Parse("0F000000-0000-0000-0000-000000000000"), 10));
        }

        [Fact]
        public void GetShardResolver_IncorrectType()
        {
            // act
            var result = Record.Exception(() => SenderBus.GetShardResolver(typeof(double), null));

            // assert
            Assert.IsType<ArgumentException>(result);
            Assert.Equal("No compatible type for default shard resolver method!", result.Message);
        }

        #endregion

        #region MessagePriorityToByte

        [Theory]
        [InlineData(MessagePriority.NormalPriority, 0)]
        [InlineData(MessagePriority.Level1Priority, 1)]
        [InlineData(MessagePriority.Level2Priority, 2)]
        [InlineData(MessagePriority.Level3Priority, 3)]
        [InlineData(MessagePriority.Level4Priority, 4)]
        public void MessagePriorityToByte(MessagePriority priority, byte expectedPriority)
        {
            // act
            var result = SenderBus.MessagePriorityToByte(priority);

            // assert
            Assert.Equal(expectedPriority, result);
        }

        #endregion
    }
}