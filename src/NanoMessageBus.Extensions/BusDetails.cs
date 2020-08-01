namespace NanoMessageBus.Extensions
{
    using System.Runtime.CompilerServices;

    public static class BusDetails
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetExchangeName(string identification) => $"exchange.{identification}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetRoutingKey(string identification, int i) => $"routingkey.{identification}.{i}.async";
    }
}
