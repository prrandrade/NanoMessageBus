namespace NanoMessageBus.Sender.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Abstractions.Attributes;
    using Abstractions.Enums;
    using Abstractions.Interfaces;
    using DateTimeUtils.Interfaces;
    using Extensions;
    using Interfaces;
    using PropertyRetriever.Interfaces;
    using RabbitMQ.Client;

    public class SenderBus : ISenderBus
    {
        // injected dependencies
        public ILoggerFacade<SenderBus> Logger { get; }
        public IDateTimeUtils DateTimeUtils { get; }

        // private fields - configuration
        public int MaxShardingSize { get; }
        public string Identification { get; }

        // private fields - rabbitmq
        public IConnection Connection { get; }

        public SenderBus(ILoggerFacade<SenderBus> logger, IRabbitMqConnectionFactoryManager connectionFactoryManager, IPropertyRetriever propertyRetriever, IDateTimeUtils dateTimeUtils)
        {
            try
            {
                Logger = logger;
                DateTimeUtils = dateTimeUtils;

                #region Getting Properties from command line or environment
                Identification = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerIdentification", variableName: "brokerIdentification", fallbackValue: "NoIdentification");
                MaxShardingSize = propertyRetriever.RetrieveFromEnvironment(variableName: "brokerMaxShardingSize", fallbackValue: 1);
                Logger.LogDebug($"Sending with ServiceIdentification: {Identification}");
                Logger.LogDebug($"Sending with MaxShardingSize: {MaxShardingSize}");
                #endregion

                #region Creating RabbitMQ Connection
                var hostnames = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerHostname", variableName: "brokerHostname", fallbackValue: "localhost:5672");
                var virtualHost = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerVirtualHost", variableName: "brokerVirtualHost", fallbackValue: "/");
                var username = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerUsername", variableName: "brokerUsername", fallbackValue: "guest");
                var password = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerPassword", variableName: "brokerPassword", fallbackValue: "guest");
                var connectionFactory = connectionFactoryManager.GetConnectionFactory(username, virtualHost, password, true);

                Connection = connectionFactory.CreateConnection(hostnames.Split(','));
                Logger.LogDebug($"Connecting to servers: {hostnames}");
                #endregion

                #region Registering Exchange

                using var channel = Connection.CreateModel();
                for (var i = 0; i < MaxShardingSize; i++)
                {
                    var exchange = BusDetails.GetExchangeName(Identification, (uint)i);
                    channel.ExchangeDeclare(exchange, ExchangeType.Fanout, true);
                    Logger.LogDebug($"Creating fanout exchange {exchange} to send messages.");
                }
                channel.Close();

                #endregion
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("A error has occurred, impossible to continue. Please see the inner exception for details.", ex);
            }
        }

        public async Task SendAsync(IMessage message, MessagePriority priority = MessagePriority.NormalPriority, Func<object, int, int> shardResolver = null)
        {
            using var ch = Connection.CreateModel();

            // resolving dependencies
            var (messageIdType, messageId) = GetMessageId(message);
            var shardFuncResolver = GetShardResolver(messageIdType, shardResolver);

            // getting type of message
            var messageType = message.GetType();
            var fullName = messageType.AssemblyQualifiedName;

            // getting basic properties
            var basicProperties = ch.CreateBasicProperties();
            basicProperties.Headers = new Dictionary<string, object> { { "prepareToSendAt", DateTimeUtils.UtcNow().ToBinary() } };
            basicProperties.DeliveryMode = 2;
            basicProperties.Type = fullName;
            basicProperties.Persistent = true;
            basicProperties.Priority = MessagePriorityToByte(priority);

            // discovering the message shard, and calculating the destiny queue
            var shardResolverResult = shardFuncResolver(messageId, MaxShardingSize);
            var exchange = string.Format(BusDetails.GetExchangeName(Identification, (uint)shardResolverResult));

            // sending the message
            var stream = new MemoryStream();
            await System.Text.Json.JsonSerializer.SerializeAsync(stream, message, message.GetType());
            var byteContent = stream.ToArray();
            basicProperties.Headers.Add("sentAt", DateTimeUtils.UtcNow().ToBinary());
            ch.BasicPublish(exchange, string.Empty, basicProperties, byteContent);
            Logger.LogDebug($"Sending message {messageType.Name} to {exchange}");
            ch.Close();
        }

        #region Private methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (Type, object) GetMessageId(IMessage message)
        {
            var messageId = (from property in message.GetType().GetProperties()
                             from attrib in property.GetCustomAttributes(typeof(MessageIdAttribute), false).Cast<MessageIdAttribute>()
                             select property).FirstOrDefault()?.GetValue(message);

            if (messageId == null)
            {
                var errMessage = $"No {nameof(MessageIdAttribute)} property was found!";
                Logger.LogError(errMessage);
                throw new ArgumentException(errMessage);
            }

            var type = messageId.GetType();
            if (type != typeof(int) && type != typeof(Guid))
            {
                var errMessage = $"Incompatible type for property with {nameof(MessageIdAttribute)} property. Only {nameof(Int32)} or {nameof(Guid)} types are valid.";
                Logger.LogError(errMessage);
                throw new ArgumentException(errMessage);
            }

            return (type, messageId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Func<object, int, int> GetShardResolver(Type messageIdType, Func<object, int, int> shardResolver)
        {
            if (shardResolver == null)
            {
                if (messageIdType == typeof(int))
                    return (mId, size) => (int)mId % size;
                else if (messageIdType == typeof(Guid))
                    return (mId, size) => ((Guid)mId).ToByteArray().Aggregate(0, (current, byteElement) => current + byteElement) % size;
                else
                {
                    const string errMessage = "No compatible type for default shard resolver method!";
                    Logger.LogError(errMessage);
                    throw new ArgumentException(errMessage);
                }
            }
            else
            {
                return shardResolver;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte MessagePriorityToByte(MessagePriority messagePriority)
        {
            return messagePriority switch
            {
                MessagePriority.NormalPriority => 0,
                MessagePriority.Level1Priority => 1,
                MessagePriority.Level2Priority => 2,
                MessagePriority.Level3Priority => 3,
                MessagePriority.Level4Priority => 4,
                _ => throw new NotImplementedException()
            };
        }

        #endregion
    }
}
