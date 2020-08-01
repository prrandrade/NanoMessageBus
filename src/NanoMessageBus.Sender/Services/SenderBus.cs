namespace NanoMessageBus.Sender.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Abstractions;
    using DateTimeUtils.Interefaces;
    using Extensions;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using PropertyRetriever.Interfaces;
    using RabbitMQ.Client;

    public class SenderBus : ISenderBus
    {
        private ILogger<SenderBus> Logger { get; }
        private IConnectionFactory ConnectionFactory { get; }
        private IConnection Connection { get; }
        private IModel Channel { get; }
        private IDateTimeUtils DateTimeUtils { get; }

        private int MaxShardingSize { get; }
        private string ServiceIdentification { get; }

        public SenderBus(ILogger<SenderBus> logger, IPropertyRetriever propertyRetriever, IDateTimeUtils dateTimeUtils)
        {
            Logger = logger;
            DateTimeUtils = dateTimeUtils;

            #region Getting Properties from command line or environment
            ServiceIdentification = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerIdentification", variableName: "brokerIdentification");
            MaxShardingSize = MaxShardingSize = propertyRetriever.RetrieveFromEnvironment(variableName: "brokerMaxShardingSize", fallbackValue: 1);
            Logger.LogInformation($"NanoMessageBus Sender starting with ServiceIdentification: {ServiceIdentification}");
            Logger.LogInformation($"NanoMessageBus Sender starting with MaxShardingSize: {MaxShardingSize}");
            #endregion

            #region Creating RabbitMQ Connection
            var hostnames = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerHostname", variableName: "brokerHostname", fallbackValue: "localhost:5672");
            var virtualHost = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerVirtualHost", variableName: "brokerVirtualHost", fallbackValue: "/");
            var username = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerUsername", variableName: "brokerUsername", fallbackValue: "guest");
            var password = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerPassword", variableName: "brokerPassword", fallbackValue: "guest");
            ConnectionFactory = new ConnectionFactory
            {
                UserName = username,
                VirtualHost = virtualHost,
                Password = password,
                AutomaticRecoveryEnabled = true
            };
            Connection = ConnectionFactory.CreateConnection(hostnames.Split(';'));
            Channel = Connection.CreateModel();
            Logger.LogInformation($"NanoMessageBus Sender connection to servers {hostnames}");
            #endregion

            #region Registering Exchange

            var exchange = BusDetails.GetExchangeName(ServiceIdentification);
            Channel.ExchangeDeclare(exchange, "direct", true);
            Logger.LogInformation($"NanoMessageBus Sender creating exchange {exchange} to send messages.");

            #endregion
        }

        public async Task SendAsync(IMessage message, MessagePriority priority = MessagePriority.NormalPriority, Func<object, int, int> shardResolver = null)
        {
            // resolving dependencies
            var (messageIdType, messageId) = GetMessageId(message);
            var shardFuncResolver = GetShardResolver(messageIdType, shardResolver);

            // getting type of message and 
            var messageType = message.GetType();
            var fullName = messageType.AssemblyQualifiedName ?? "";

            // getting basic properties
            var basicProperties = Channel.CreateBasicProperties();
            basicProperties.Headers = new Dictionary<string, object> { { "SendStartDate", DateTimeUtils.UtcNow().ToBinary() } };
            basicProperties.DeliveryMode = 2;
            basicProperties.Type = fullName;
            basicProperties.Persistent = true;
            basicProperties.Priority = MessagePriorityToByte(priority);

            // discovering the message shard, and calculating the exchange and the routing key
            var shardResolverResult = shardFuncResolver(messageId, MaxShardingSize);
            var exchange = BusDetails.GetExchangeName(ServiceIdentification);
            var routingKey = BusDetails.GetRoutingKey(ServiceIdentification, shardResolverResult);

            // sending the message
            var byteContent = await CustomSerializer.CompressMessageAsync(message);
            using var ch = Connection.CreateModel();
            basicProperties.Headers.Add("SendFinishDate", DateTimeUtils.UtcNow().ToBinary());
            ch.BasicPublish(exchange, routingKey, basicProperties, byteContent);
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
                    return (mId, size) => (int) mId % size;
                else if (messageIdType == typeof(Guid))
                    return (mId, size) => ((Guid) mId).ToByteArray().Aggregate(0, (current, byteElement) => current + byteElement) % size;
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
        internal static byte MessagePriorityToByte(MessagePriority messagePriority)
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
