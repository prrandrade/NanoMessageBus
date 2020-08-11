using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NanoMessageBus.Receiver.Test")]
namespace NanoMessageBus.Receiver.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Abstractions.Interfaces;
    using DateTimeUtils.Interfaces;
    using Extensions;
    using Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using PropertyRetriever.Interfaces;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using static Abstractions.LocalConstants;

    public class ReceiverBus : IReceiverBus
    {
        // injected dependencies
        public ILoggerFacade<ReceiverBus> Logger { get; }
        public IDateTimeUtils DateTimeUtils { get; }
        public ICompressor Compressor { get; }
        public IServiceScopeFactory ServiceScopeFactory { get; }

        // properties to control consumers and queues for this consumer
        public Dictionary<Type, Type> MessageTypes { get; } = new Dictionary<Type, Type>();
        public List<string> Queues { get; } = new List<string>();
        public Dictionary<string, IModel> Channels { get; } = new Dictionary<string, IModel>();
        public Dictionary<string, EventingBasicConsumer> Consumers { get; } = new Dictionary<string, EventingBasicConsumer>();
        public List<string> ListenedServices { get; }
        public List<uint> ListenedShards { get; }

        // private fields - rabbitmq
        private IConnectionFactory ConnectionFactory { get; }

        public ReceiverBus(ILoggerFacade<ReceiverBus> logger, IRabbitMqConnectionFactoryManager connectionFactoryManager,
            IRabbitMqEventingBasicConsumerManager basicConsumerManager, IServiceScopeFactory serviceScopeFactory, 
            IPropertyRetriever propertyRetriever, IDateTimeUtils dateTimeUtils, ICompressor compressor,
            IEnumerable<IMessageHandler> handlers)
        {
            try
            {
                Logger = logger;
                DateTimeUtils = dateTimeUtils;
                ServiceScopeFactory = serviceScopeFactory;
                Compressor = compressor;

                #region Getting Properties from command line or environment

                var identification = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: BrokerIdentificationProperty, variableName: BrokerIdentificationProperty, fallbackValue: BrokerIdentificationFallbackValue);
                var maxShardingSize = propertyRetriever.RetrieveFromEnvironment(variableName: BrokerMaxShardingSizeProperty, fallbackValue: BrokerMaxShardingSizeFallbackValue);
                if (maxShardingSize <= 0)
                {
                    Logger.LogWarning($"Property {BrokerIdentificationProperty} is invalid, will be treated as 1!");
                    maxShardingSize = 1;
                }

                ListenedServices = BusDetails.GetListenedServicesFromPropertyValue(propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: BrokerListenedServicesProperty, variableName: BrokerListenedServicesProperty, fallbackValue: string.Format(BrokerListenedServicesFallbackValue, identification)));
                ListenedShards = BusDetails.GetListenedShardsFromPropertyValue(propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: BrokerListenedShardsProperty, variableName: BrokerListenedShardsProperty, fallbackValue: string.Format(BrokerListenedShardsFallbackValue, maxShardingSize - 1)), (uint)maxShardingSize);

                Logger.LogDebug($"Receiving with MaxShardingSize: {maxShardingSize}");
                Logger.LogDebug($"Listening Services {string.Join(',', ListenedServices)}");
                Logger.LogDebug($"Listening Shards {string.Join(',', ListenedShards)}");

                #endregion

                #region Loading Handlers

                foreach (var handler in handlers)
                {
                    var messageType = handler.GetType().GetInterfaces()[0].GetGenericArguments()[0];
                    if (!MessageTypes.ContainsKey(messageType))
                    {
                        MessageTypes.Add(messageType, handler.GetType());
                        Logger.LogDebug($"Found handler {handler.GetType().Name} for message {messageType.Name}");
                    }
                }

                #endregion

                #region Creating Connection

                var hostnames = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: BrokerHostnameProperty, variableName: BrokerHostnameProperty, fallbackValue: BrokerHostnameFallbackValue);
                var virtualHost = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: BrokerVirtualHostProperty, variableName: BrokerVirtualHostProperty, fallbackValue: BrokerVirtualHostFallbackValue);
                var username = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: BrokerUsernameProperty, variableName: BrokerUsernameProperty, fallbackValue: BrokerUsernameFallbackValue);
                var password = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: BrokerPasswordProperty, variableName: BrokerPasswordProperty, fallbackValue: BrokerPasswordFallbackValue);
                var prefetchSize = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: BrokerPrefetchSizeProperty, variableName: BrokerPrefetchSizeProperty, fallbackValue: BrokerPrefetchSizeFallbackValue);
                ConnectionFactory = connectionFactoryManager.GetConnectionFactory(username, virtualHost, password, true);
                var connection = ConnectionFactory.CreateConnection(hostnames.Split(','));

                #endregion

                #region Creating the queues and binding with the exchanges

                using (var baseChannel = connection.CreateModel())
                {
                    foreach (var listenedShard in ListenedShards)
                    {
                        var queue = BusDetails.GetQueueName(identification, listenedShard);
                        Queues.Add(queue);
                        baseChannel.QueueDeclare(queue, true, false, false);

                        foreach (var listenedService in ListenedServices)
                        {
                            var exchange = BusDetails.GetExchangeName(listenedService, listenedShard);
                            baseChannel.ExchangeDeclare(exchange, ExchangeType.Fanout, true);
                            baseChannel.QueueBind(queue, exchange, string.Empty);
                            Logger.LogDebug($"Binding Exchange {exchange} with Queue {queue}.");
                        }
                    }

                    baseChannel.Close();
                }

                #endregion

                #region Creating individual channel for each queue

                foreach (var queue in Queues)
                {
                    var channel = connection.CreateModel();
                    channel.BasicQos(0, prefetchSize, true);
                    Channels.Add(queue, channel);
                    var asyncConsumer = basicConsumerManager.GetNewEventingBasicConsumer(channel);

                    asyncConsumer.Received += async (o, ea) => await ConsumeMessageAsync(channel, ea);
                    Consumers.Add(queue, asyncConsumer);
                    Logger.LogDebug($"Preparing to consume queue {queue}.");
                }

                #endregion
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("A error has occurred, impossible to continue. Please see the inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Start consuming messages
        /// </summary>
        public void StartConsumer()
        {
            foreach (var queue in Queues)
            {
                Channels[queue].BasicConsume(queue, true, Consumers[queue]);
                Logger.LogDebug($"Consuming RabbitMQ queue {queue}.");
            }
        }

        internal async Task ConsumeMessageAsync(IModel channel, BasicDeliverEventArgs ea)
        {
            var prepareToSendAt = (long)ea.BasicProperties.Headers["prepareToSendAt"];
            var sentAt = (long)ea.BasicProperties.Headers["sentAt"];
            var receivedAt = DateTimeUtils.UtcNow().ToBinary();

            var (receivedConvertedMessage, receivedMessageType) = await ProcessDeliveredMessageAsync(ea);
            if (receivedConvertedMessage == null) return;

            var handlerType = MessageTypes[receivedMessageType];
            await ProcessReceivedMessageAsync(prepareToSendAt, sentAt, receivedAt, receivedConvertedMessage, handlerType);
        }

        private async Task<(IMessage, Type)> ProcessDeliveredMessageAsync(BasicDeliverEventArgs ea)
        {
            var receivedMessageType = Type.GetType(ea.BasicProperties.Type);
            if (receivedMessageType == null)
            {
                Logger.LogWarning($"Unrecognizable type {ea.BasicProperties.Type} for delivered message!");
                return (null, null);
            }

            if (!MessageTypes.ContainsKey(receivedMessageType))
            {
                Logger.LogWarning($"There's no handler for {ea.BasicProperties.Type}. This message will be ignored!");
                return (null, null);
            }

            var receivedMessage = await Compressor.DecompressMessageAsync(ea.Body.ToArray(), receivedMessageType);
            var receivedConvertedMessage = (IMessage)receivedMessage;

            return (receivedConvertedMessage, receivedMessageType);
        }

        private async Task ProcessReceivedMessageAsync(long prepareToSendAt, long sentAt, long receivedAt, IMessage receivedConvertedMessage, Type handlerType)
        {
            using var scope = ServiceScopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetService(handlerType);
            var registerStatistics = handlerType.GetMethod(nameof(IMessageHandler<IMessage>.RegisterStatisticsAsync));
            var beforeHandle = handlerType.GetMethod(nameof(IMessageHandler<IMessage>.BeforeHandleAsync));
            var handle = handlerType.GetMethod(nameof(IMessageHandler<IMessage>.HandleAsync));
            var afterHandle = handlerType.GetMethod(nameof(IMessageHandler<IMessage>.AfterHandleAsync));

            var statisticsArguments = new object[] { DateTime.FromBinary(prepareToSendAt), DateTime.FromBinary(sentAt), DateTime.FromBinary(receivedAt), DateTimeUtils.UtcNow() };
            var arguments = new object[] { receivedConvertedMessage };

            // ReSharper disable PossibleNullReferenceException
            await (Task)registerStatistics.Invoke(handler, statisticsArguments);
            var preResult = await (Task<bool>)beforeHandle.Invoke(handler, arguments);
            if (preResult) await (Task)handle.Invoke(handler, arguments);
            await (Task)afterHandle.Invoke(handler, arguments);
            // ReSharper restore PossibleNullReferenceException
        }
    }
}
