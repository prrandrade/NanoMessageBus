namespace NanoMessageBus.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;

    public static class BusDetails
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetExchangeName(string identification, int i) => $"exchange.{identification}.{i}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetQueueName(string identification, int i) => $"queue.{identification}.{i}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<int> GetListenedShardsFromPropertyValue(string shardCommand, int maxShard)
        {
            var list = new List<int>();

            foreach (var shardCommandParameter in shardCommand.Split(','))
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

            return list.Distinct().OrderBy(x => x).ToList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<string> GetListenedServicesFromPropertyValue(string serviceCommand) => serviceCommand.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}
