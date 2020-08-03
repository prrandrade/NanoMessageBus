namespace NanoMessageBus.Receiver.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using Abstractions;
    using DateTimeUtils.Interfaces;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using PropertyRetriever.Interfaces;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    public class ReceiverBus
    {
        // injected dependencies
        private ILogger<ReceiverBus> Logger { get; }
        private IPropertyRetriever PropertyRetriever { get; }
        private IDateTimeUtils DateTimeUtils { get; }
        private IEnumerable<IMessageHandler> Handlers { get; }

        // private fields
        private readonly int _maxShardingSize;
        private readonly string _identification;
        private readonly List<int> _listenedShards;


        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler<MessageEventArgs> MessageProcessed;


        private IConnectionFactory ConnectionFactory { get; }
        private IConnection Connection { get; }
        private IModel Channel { get; }

        private Dictionary<string, IModel> Channels { get; }
        private Dictionary<string, EventingBasicConsumer> Consumers { get; }

        public string ServiceIdentification { get; }
        public int MaxShardingSize { get; }
        public bool AutoAck { get; }

        public Dictionary<string, List<int>> ListenedServices { get; } = new Dictionary<string, List<int>>();

        public ReceiverBus(ILogger<ReceiverBus> logger, IPropertyRetriever propertyRetriever, IDateTimeUtils dateTimeUtils,
            IEnumerable<IMessageHandler> handlers)
        {
            Logger = logger;
            PropertyRetriever = propertyRetriever;
            DateTimeUtils = dateTimeUtils;

            #region Getting Properties from command line or environment
            _identification = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerIdentification", variableName: "brokerIdentification");
            _maxShardingSize = propertyRetriever.RetrieveFromEnvironment(variableName: "brokerMaxShardingSize", fallbackValue: 1);
            _listenedShards = GetShardsFromPropertyValue(propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerListenedShards", variableName: "brokerListenedShards", fallbackValue: "1"), _maxShardingSize);

            #endregion




            ServiceIdentification = propertyRetriever.RetrieveFromCommandLineOrEnvironment(longName: "brokerIdentification", variableName: "brokerIdentification");


        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<int> GetShardsFromPropertyValue(string shardCommand, int maxShard)
        {
            var list = new List<int>();

            foreach (var shardCommandParameter in shardCommand.Split(';'))
            {
                // todo checar se o parâmetro é maior que o shard maximo
                if (Regex.IsMatch(shardCommandParameter, @"^\d+$"))
                {
                    list.Add(Convert.ToInt32(shardCommandParameter));
                }
                else if (Regex.IsMatch(shardCommandParameter, @"^\d+-\d+$"))
                {
                    // todo checar se os parâmetros são maior que o shard maximo
                    var min = Convert.ToInt32(shardCommandParameter.Split('-')[0]);
                    var max = Convert.ToInt32(shardCommandParameter.Split('-')[1]);

                    for (var i = min; i <= max; i++)
                        list.Add(i);
                }
            }

            return list.Distinct().OrderBy(x => x);
        }
    }
}
