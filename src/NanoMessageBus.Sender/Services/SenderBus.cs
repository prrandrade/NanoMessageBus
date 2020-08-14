using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NanoMessageBus.Sender.Test")]
namespace NanoMessageBus.Sender.Services
{
    using System;
    using System.Collections.Generic;
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
    using static Abstractions.LocalConstants;

    public class SenderBus : ISenderBus
    {
        // injected dependencies
        public ILoggerFacade<SenderBus> Logger { get; }
        public IDateTimeUtils DateTimeUtils { get; }
        public ISerialization Serializer { get; set; }

        // private fields - configuration
        public int MaxShardingSize { get; }
        public string Identification { get; }

        // private fields - rabbitmq
        public IConnection Connection { get; }

        public SenderBus(ILoggerFacade<SenderBus> logger, IRabbitMqConnectionFactoryManager connectionFactoryManager, 
            IPropertyRetriever propertyRetriever, IDateTimeUtils dateTimeUtils, ISerialization serializer)
        {
            try
            {
                Logger = logger;
                DateTimeUtils = dateTimeUtils;
                Serializer = serializer;

                #region Getting Properties from command line or environment
                Identification = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: BrokerIdentificationProperty, variableName: BrokerIdentificationProperty, fallbackValue: BrokerIdentificationFallbackValue);
                MaxShardingSize = propertyRetriever.RetrieveFromEnvironment(variableName: BrokerMaxShardingSizeProperty, fallbackValue: BrokerMaxShardingSizeFallbackValue);
                if (MaxShardingSize <= 0)
                {
                    Logger.LogWarning($"Property {BrokerIdentificationProperty} is invalid, will be treated as 1!");
                    MaxShardingSize = 1;
                }
                Logger.LogDebug($"Sending with ServiceIdentification: {Identification}");
                Logger.LogDebug($"Sending with MaxShardingSize: {MaxShardingSize}");
                #endregion

                #region Creating RabbitMQ Connection
                var hostnames = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: BrokerHostnameProperty, variableName: BrokerHostnameProperty, fallbackValue: BrokerHostnameFallbackValue);
                var virtualHost = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: BrokerVirtualHostProperty, variableName: BrokerVirtualHostProperty, fallbackValue: BrokerVirtualHostFallbackValue);
                var username = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: BrokerUsernameProperty, variableName: BrokerUsernameProperty, fallbackValue: BrokerUsernameFallbackValue);
                var password = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: BrokerPasswordProperty, variableName: BrokerPasswordProperty, fallbackValue: BrokerPasswordFallbackValue);
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

        /// <summary>
        /// Send a message via RabbitMQ to all listening services.
        /// </summary>
        /// <param name="message">Message that will be sent.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SendAsync(IMessage message)
        {
            await SendAsync(message, MessagePriority.NormalPriority, null);
        }

        /// <summary>
        /// Send a message via RabbitMQ to all listening services.
        /// </summary>
        /// <param name="message">Message that will be sent.</param>
        /// <param name="priority">Message priority. Messages with more priority are processed earlier.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SendAsync(IMessage message, MessagePriority priority)
        {
            await SendAsync(message, priority, null);
        }

        /// <summary>
        /// Send a message via RabbitMQ to all listening services.
        /// </summary>
        /// <param name="message">Message that will be sent.</param>
        /// <param name="shardResolver">Customized function to decide which shard will be used. The first parameter must be converted to Guid or Int.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SendAsync(IMessage message, Func<object, int, int> shardResolver)
        {
            await SendAsync(message, MessagePriority.NormalPriority, shardResolver);
        }

        /// <summary>
        /// Send a message via RabbitMQ to all listening services.
        /// </summary>
        /// <param name="message">Message that will be sent.</param>
        /// <param name="priority">Message priority. Messages with more priority are processed earlier.</param>
        /// <param name="shardResolver">Customized function to decide which shard will be used. The first parameter must be converted to Guid or Int.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SendAsync(IMessage message, MessagePriority priority, Func<object, int, int> shardResolver)
        {
            try
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
                var byteContent = await Serializer.SerializeMessageAsync(message);
                basicProperties.Headers.Add("sentAt", DateTimeUtils.UtcNow().ToBinary());
                ch.BasicPublish(exchange, string.Empty, basicProperties, byteContent);
                Logger.LogDebug($"Sending message {messageType.Name} to {exchange}");
                ch.Close();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("A error has occurred, impossible to continue. Please see the inner exception for details.", ex);
            }
        }

        #region Internal methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static (Type, object) GetMessageId(IMessage message)
        {
            var messageId = (from property in message.GetType().GetProperties()
                             from attrib in property.GetCustomAttributes(typeof(MessageIdAttribute), false).Cast<MessageIdAttribute>()
                             select property).FirstOrDefault()?.GetValue(message);

            if (messageId == null)
            {
                var errMessage = $"No {nameof(MessageIdAttribute)} property was found!";
                throw new ArgumentException(errMessage);
            }

            var type = messageId.GetType();
            if (type != typeof(int) && type != typeof(Guid))
            {
                var errMessage = $"Incompatible type for property with {nameof(MessageIdAttribute)} property. Only {nameof(Int32)} or {nameof(Guid)} types are valid.";
                throw new ArgumentException(errMessage);
            }

            return (type, messageId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Func<object, int, int> GetShardResolver(Type messageIdType, Func<object, int, int> shardResolver)
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
            return (byte) messagePriority;
        }

        #endregion
    }
}
