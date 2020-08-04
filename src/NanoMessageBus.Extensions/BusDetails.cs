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
        public static string GetExchangeName(string identification, uint i) => $"exchange.{identification}.{i}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetQueueName(string identification, uint i) => $"queue.{identification}.{i}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<uint> GetListenedShardsFromPropertyValue(string shardCommand, uint maxShard)
        {
            var list = new List<uint>();

            foreach (var shardCommandParameter in shardCommand.Split(','))
            {
                if (Regex.IsMatch(shardCommandParameter, @"^\d+$"))
                {
                    var shardNum = Convert.ToUInt32(shardCommandParameter);

                    if (shardNum >= maxShard)
                        throw new ArgumentException($"Invalid shard {shardCommandParameter}. It must be less than maxShard {maxShard}!");

                    list.Add(shardNum);
                }
                else if (Regex.IsMatch(shardCommandParameter, @"^\d+-\d+$"))
                {
                    var min = Convert.ToUInt32(shardCommandParameter.Split('-')[0]);
                    var max = Convert.ToUInt32(shardCommandParameter.Split('-')[1]);

                    if (min > max)
                        throw new ArgumentException($"Invalid interval {shardCommandParameter}!");
                    if (min >= maxShard || max >= maxShard)
                        throw new ArgumentException($"Invalid interval {shardCommandParameter}. The interval must be less than maxShard {maxShard}!");

                    for (var i = min; i <= max; i++)
                        list.Add(i);
                }
            }

            return list.Distinct().OrderBy(x => x).ToList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<string> GetListenedServicesFromPropertyValue(string serviceCommand) => serviceCommand.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
    }
}
