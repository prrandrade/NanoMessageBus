﻿namespace NanoMessageBus.Receiver.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Abstractions;
    using DateTimeUtils.Interfaces;
    using Extensions;
    using Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using PropertyRetriever.Interfaces;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    public class ReceiverBus : IReceiverBus
    {
        // injected dependencies
        private ILogger<ReceiverBus> Logger { get; }
        private IDateTimeUtils DateTimeUtils { get; }

        // properties to control consumers and queues for this consumer
        private Dictionary<Type, Type> MessageTypes { get; } = new Dictionary<Type, Type>();
        private List<string> Queues { get; } = new List<string>();
        private Dictionary<string, IModel> Channels { get; } = new Dictionary<string, IModel>();
        private Dictionary<string, EventingBasicConsumer> Consumers { get; } = new Dictionary<string, EventingBasicConsumer>();

        // private fields - configuration
        private readonly bool _autoAck;

        // private fields - rabbitmq
        private IConnectionFactory ConnectionFactory { get; }
        private IConnection Connection { get; }
        private IModel Channel { get; }

        public ReceiverBus(ILogger<ReceiverBus> logger, IServiceScopeFactory serviceScopeFactory,
            IPropertyRetriever propertyRetriever, IDateTimeUtils dateTimeUtils,
            IEnumerable<IMessageHandler> handlers)
        {
            Logger = logger;
            DateTimeUtils = dateTimeUtils;

            #region Getting Properties from command line or environment
            var identification = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerIdentification", variableName: "brokerIdentification");
            var maxShardingSize = propertyRetriever.RetrieveFromEnvironment(variableName: "brokerMaxShardingSize", fallbackValue: 1);
            var listenedServices = BusDetails.GetListenedServicesFromPropertyValue(propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerListenedServices", variableName: "brokerListenedServices"));
            var listenedShards = BusDetails.GetListenedShardsFromPropertyValue(propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerListenedShards", variableName: "brokerListenedShards", fallbackValue: "1"), maxShardingSize);
            _autoAck = propertyRetriever.CheckFromCommandLine("autoAck");
            Logger.LogDebug($"Receiving with MaxShardingSize: {maxShardingSize}");
            Logger.LogDebug($"Listening Services {string.Join(',', listenedServices)}");
            Logger.LogDebug($"Listening Shards {string.Join(',', listenedShards)}");
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
                else
                {
                    Logger.LogWarning($"Handler {handler.GetType().Name} will be ignored! Message {messageType.Name} will be handled by {MessageTypes[messageType]}");
                }
            }
            #endregion

            #region Creating Connection

            var hostnames = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerHostname", variableName: "brokerHostname", fallbackValue: "localhost:5672");
            var virtualHost = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerVirtualHost", variableName: "brokerVirtualHost", fallbackValue: "/");
            var username = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerUsername", variableName: "brokerUsername", fallbackValue: "guest");
            var password = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerPassword", variableName: "brokerPassword", fallbackValue: "guest");
            var prefetchSize = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerPrefetchSize", variableName: "brokerPrefetchSize", fallbackValue: (ushort)100);

            ConnectionFactory = new ConnectionFactory
            {
                UserName = username,
                VirtualHost = virtualHost,
                Password = password,
                AutomaticRecoveryEnabled = true
            };
            Connection = ConnectionFactory.CreateConnection(hostnames.Split(','));
            Channel = Connection.CreateModel();
            Channel.BasicQos(0, prefetchSize, true);

            #endregion

            #region Creating the queues and binding with the exchanges

            foreach (var listenedShard in listenedShards)
            {
                var queue = BusDetails.GetQueueName(identification, listenedShard);
                Queues.Add(queue);
                foreach (var listenedService in listenedServices)
                {
                    var exchange = BusDetails.GetExchangeName(listenedService, listenedShard);
                    Channel.QueueDeclare(queue, true, false, false);
                    Channel.ExchangeDeclare(exchange, ExchangeType.Fanout, true);
                    Channel.QueueBind(queue, exchange, string.Empty);
                    Logger.LogDebug($"Binding Exchange {exchange} with Queue {queue}");
                }
            }

            #endregion

            #region Creating individual channel for each queue

            foreach (var queue in Queues)
            {
                var channel = Connection.CreateModel();
                Channels.Add(queue, channel);
                var asyncConsumer = new EventingBasicConsumer(channel);
                asyncConsumer.Received += async (sender, ea) =>
                {
                    try
                    {
                        var prepareToSendAt = (long)ea.BasicProperties.Headers["prepareToSendAt"];
                        var sentAt = (long)ea.BasicProperties.Headers["sentAt"];
                        var receivedAt = DateTimeUtils.UtcNow().ToBinary();

                        var (receivedConvertedMessage, receivedMessageType) = await ProcessDeliveredMessage(ea);
                        if (receivedConvertedMessage == null) return;

                        var handlerType = MessageTypes[receivedMessageType];
                        await ProcessReceivedMessage(prepareToSendAt, sentAt, receivedAt, serviceScopeFactory, receivedConvertedMessage, handlerType);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Unsupported message received!");
                        throw;
                    }
                    finally
                    {
                        if (!_autoAck)
                            channel.BasicAck(ea.DeliveryTag, false);
                    }
                };
                Consumers.Add(queue, asyncConsumer);
                Logger.LogDebug($"Preparing to consume queue {queue}");
            }
            #endregion
        }

        public void StartConsumer()
        {
            foreach (var queue in Queues)
            {
                Channels[queue].BasicConsume(queue, _autoAck, Consumers[queue]);
                Logger.LogDebug($"Consuming RabbitMQ queue {queue}.");
            }
        }

        private async Task<(IMessage, Type)> ProcessDeliveredMessage(BasicDeliverEventArgs ea)
        {
            var receivedMessageType = Type.GetType(ea.BasicProperties.Type);
            if (receivedMessageType == null)
            {
                Logger.LogWarning($"Unrecognizable type {ea.BasicProperties.Type} for delivered message!");
                return (null, null);
            }

            if (!MessageTypes.ContainsKey(receivedMessageType))
            {
                Logger.LogDebug($"There's no handler for {ea.BasicProperties.Type}. This message will be ignored!");
                return (null, null);
            }

            var receivedMessage = await CustomSerializer.DecompressMessageAsync(receivedMessageType, ea.Body.ToArray());
            var receivedConvertedMessage = (IMessage)receivedMessage;

            return (receivedConvertedMessage, receivedMessageType);
        }

        private async Task ProcessReceivedMessage(long prepareToSendAt, long sentAt, long receivedAt, IServiceScopeFactory serviceScopeFactory, IMessage receivedConvertedMessage, Type handlerType)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetService(handlerType);
            var registerStatistics = handlerType.GetMethod("RegisterStatistics");
            var beforeHandle = handlerType.GetMethod("BeforeHandle");
            var handle = handlerType.GetMethod("Handle");
            var afterHandle = handlerType.GetMethod("AfterHandle");

            if (registerStatistics == null || beforeHandle == null || handle == null || afterHandle == null)
                throw new ArgumentException("Error while retrieving handle methods, message will not be processed!");

            if (registerStatistics.ReturnType != typeof(Task))
                throw new ArgumentException("Error with registerStatistics return method, message will not be processed!");
            if (beforeHandle.ReturnType != typeof(Task<bool>))
                throw new ArgumentException("Error with beforeHandle return method, message will not be processed!");
            if (handle.ReturnType != typeof(Task))
                throw new ArgumentException("Error with handle return method, message will not be processed!");
            if (afterHandle.ReturnType != typeof(Task))
                throw new ArgumentException("Error with afterHandle return method, message will not be processed!");

            var statisticsArguments = new object[] { prepareToSendAt, sentAt, receivedAt, DateTimeUtils.UtcNow().ToBinary() };
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
