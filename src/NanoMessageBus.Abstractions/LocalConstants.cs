namespace NanoMessageBus.Abstractions
{
    public static class LocalConstants
    {
        public const string BrokerIdentificationProperty = "brokerIdentification";
        public const string BrokerIdentificationFallbackValue = "NoIdentification";

        public const string BrokerMaxShardingSizeProperty = "brokerMaxShardingSize";
        public const int BrokerMaxShardingSizeFallbackValue = 10;

        public const string BrokerListenedServicesProperty = "brokerListenedServices";
        public const string BrokerListenedServicesFallbackValue = "{0}";

        public const string BrokerListenedShardsProperty = "brokerListenedShards";
        public const string BrokerListenedShardsFallbackValue = "0-{0}";

        public const string BrokerAutoAckProperty = "autoAck";

        public const string BrokerHostnameProperty = "brokerHostname";
        public const string BrokerHostnameFallbackValue = "localhost:5672";

        public const string BrokerVirtualHostProperty = "brokerVirtualHost";
        public const string BrokerVirtualHostFallbackValue = "/";

        public const string BrokerUsernameProperty = "brokerUsername";
        public const string BrokerUsernameFallbackValue = "guest";

        public const string BrokerPasswordProperty = "brokerPassword";
        public const string BrokerPasswordFallbackValue = "guest";

        public const string BrokerPrefetchSizeProperty = "brokerPrefetchSize";
        public const ushort BrokerPrefetchSizeFallbackValue = 100;
    }
}
