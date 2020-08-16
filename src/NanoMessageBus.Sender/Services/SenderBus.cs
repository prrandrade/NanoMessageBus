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
    using EventArgs;
    using Extensions;
    using Interfaces;
    using PropertyRetriever.Interfaces;
    using RabbitMQ.Client;
    using static Abstractions.LocalConstants;

    public class SenderBus : ISenderBus
    {
        // events
        public event EventHandler<MessageSentEventArgs> MessageSent;

        // injected dependencies
        public ILoggerFacade<SenderBus> Logger { get; }
        public IDateTimeUtils DateTimeUtils { get; }
        public List<ISerialization> Serializers { get; set; }

        // public properties
        public int MaxShardingSize { get; }
        public string Identification { get; }
        public ISerialization DefaultSerializationEngine { get; private set; }
        public IConnection Connection { get; }

        public SenderBus(ILoggerFacade<SenderBus> logger, IRabbitMqConnectionFactoryManager connectionFactoryManager,
            IPropertyRetriever propertyRetriever, IDateTimeUtils dateTimeUtils, IEnumerable<ISerialization> serializers)
        {
            try
            {
                Logger = logger;
                DateTimeUtils = dateTimeUtils;
                Serializers = serializers.ToList();

                DefaultSerializationEngine = Serializers.First(x => x.Identification == SerializationEngine.NativeJson);

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
        /// Set the default serialization engine for sending messages
        /// </summary>
        /// <param name="serializationEngine">Serialization engine</param>
        /// <remarks>If the choice is invalid, the fall back option will be NativeJson</remarks>
        public void SetDefaultSerializationEngine(SerializationEngine serializationEngine = SerializationEngine.NativeJson)
        {
            DefaultSerializationEngine = Serializers.FirstOrDefault(x => x.Identification == serializationEngine);
            if (DefaultSerializationEngine == null)
            {
                Logger.LogWarning($"SerializationEngine {serializationEngine} not found, falling back to {SerializationEngine.NativeJson}.");
                DefaultSerializationEngine = Serializers.First(x => x.Identification == SerializationEngine.NativeJson);
            }
        }

        /// <summary>
        /// Send a message via RabbitMQ to all listening services.
        /// </summary>
        /// <param name="message">Message that will be sent.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SendAsync(IMessage message) => await SendAsync(message, null, MessagePriority.NormalPriority, null);

        /// <summary>
        /// Send a message via RabbitMQ to all listening services.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="serializationEngine"></param>
        /// <returns></returns>
        public async Task SendAsync(IMessage message, SerializationEngine serializationEngine) => await SendAsync(message, serializationEngine, MessagePriority.NormalPriority, null);

        /// <summary>
        /// Send a message via RabbitMQ to all listening services.
        /// </summary>
        /// <param name="message">Message that will be sent.</param>
        /// <param name="priority">Message priority. Messages with more priority are processed earlier.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SendAsync(IMessage message, MessagePriority priority) => await SendAsync(message, null, priority, null);

        /// <summary>
        /// Send a message via RabbitMQ to all listening services.
        /// </summary>
        /// <param name="message">Message that will be sent.</param>
        /// <param name="shardResolver">Customized function to decide which shard will be used. The first parameter must be converted to Guid or Int.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SendAsync(IMessage message, Func<object, int, int> shardResolver) => await SendAsync(message, null, MessagePriority.NormalPriority, shardResolver);

        /// <summary>
        /// Send a message via RabbitMQ to all listening services.
        /// </summary>
        /// <param name="message">Message that will be sent.</param>
        /// <param name="serializationEngine">Serialization engine used to serialize this message (receiving services MUST have the serialization engine installed!)</param>
        /// <param name="priority">Message priority. Messages with more priority are processed earlier.</param>
        /// <param name="shardResolver">Customized function to decide which shard will be used. The first parameter must be converted to Guid or Int.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SendAsync(IMessage message, SerializationEngine? serializationEngine, MessagePriority priority, Func<object, int, int> shardResolver)
        {
            try
            {
                using var ch = Connection.CreateModel();
                ISerialization serializer;

                #region Choosing the serialization engine that will be used by this message
                if (serializationEngine == null)
                    serializer = DefaultSerializationEngine;
                else
                {
                    serializer = Serializers.FirstOrDefault(x => x.Identification == serializationEngine);
                    if (serializer == null)
                    {
                        Logger.LogWarning($"SerializationEngine {serializationEngine} not found, falling back to the default serializer.");
                        serializer = DefaultSerializationEngine;
                    }
                }
                #endregion

                #region Discovering message id and shard resolver
                var (messageIdType, messageId) = GetMessageId(message);
                var shardFuncResolver = GetShardResolver(messageIdType, shardResolver); 
                #endregion

                #region Getting type of message
                var messageType = message.GetType();
                var fullName = messageType.AssemblyQualifiedName; 
                #endregion

                #region Getting basic properties
                var basicProperties = ch.CreateBasicProperties();
                basicProperties.Headers = new Dictionary<string, object>
                {
                    { "prepareToSendAt", DateTimeUtils.UtcNow().ToBinary() },
                    { "serializer", (int)serializer.Identification }
                };
                basicProperties.DeliveryMode = 2;
                basicProperties.Type = fullName;
                basicProperties.Persistent = true;
                basicProperties.Priority = MessagePriorityToByte(priority);
                #endregion

                #region Discovering the message shard, and calculating the destiny queue
                var shardResolverResult = shardFuncResolver(messageId, MaxShardingSize);
                var exchange = string.Format(BusDetails.GetExchangeName(Identification, (uint)shardResolverResult));
                #endregion

                #region Sending the message
                var byteContent = await serializer.SerializeMessageAsync(message);
                basicProperties.Headers.Add("sentAt", DateTimeUtils.UtcNow().ToBinary());
                ch.BasicPublish(exchange, string.Empty, basicProperties, byteContent);
                Logger.LogDebug($"Sending message {messageType.Name} to {exchange}");
                ch.Close(); 
                #endregion

                MessageSent?.Invoke(this, new MessageSentEventArgs
                {
                    Message = message,
                    MessageSize = byteContent.Length,
                    MessageType = messageType
                });
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
            return (byte)messagePriority;
        }

        #endregion
    }
}
