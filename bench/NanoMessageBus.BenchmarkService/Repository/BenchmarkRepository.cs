namespace NanoMessageBus.BenchmarkService.Repository
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Interfaces;
    using LiteDB;
    using Models;

    public class BenchmarkRepository : IBenchmarkRepository
    {
        private readonly ILiteCollection<InfoModel> _infosCollection;

        public BenchmarkRepository()
        {
            var db = new LiteDatabase("database.db");
            _infosCollection = db.GetCollection<InfoModel>("infos");
            _infosCollection.DeleteAll();
        }

        public void SaveInfo(Guid messageId, int messageSize, long prepareToSentAt, long sentAt, long receivedAt, long handledAt)
        {
            _infosCollection.Insert(new InfoModel
            {
                MessageId = messageId,
                MessageSize = messageSize,
                PrepareToSendAt = prepareToSentAt,
                SentAt = sentAt,
                ReceivedAt = receivedAt,
                HandledAt = handledAt
            });
        }

        public async Task<string> ExportFilteredDataAsync(int totalMessages, string compressEngine)
        {
            var infos = _infosCollection.FindAll().ToList();
            var sendTimes = infos.Select(x => x.SendTime).OrderBy(x => x).ToList();
            var travelTimes = infos.Select(x => x.TravelTime).OrderBy(x => x).ToList();
            var totalTimes = infos.Select(x => x.TotalTime).OrderBy(x => x).ToList();

            var numbers = infos
                .OrderBy(x => x.SentAt)
                .GroupBy(x => DateTime.FromBinary(x.SentAt).ToString("HH:mm:ss"), (s, models) => new KeyValuePair<string, int>(s, models.Count()))
                .ToList();

            await using var sw = new StreamWriter($"benchmark_{compressEngine}.txt", false) { AutoFlush = true };
            await sw.WriteLineAsync($"Benchmark result for {totalMessages} messages, compressed with {compressEngine}.");
            await sw.WriteLineAsync($"Average message size: {infos.Select(x => x.MessageSize).Average()} bytes");
            await sw.WriteLineAsync("");
            await sw.WriteLineAsync("");

            await sw.WriteLineAsync("Percentile\tSend time");
            await sw.WriteLineAsync(FormattableString.CurrentCulture($"{"90",-10}\t{GetNthPercentile(sendTimes, 90)}"));
            await sw.WriteLineAsync(FormattableString.CurrentCulture($"{"95",-10}\t{GetNthPercentile(sendTimes, 95)}"));
            await sw.WriteLineAsync(FormattableString.CurrentCulture($"{"99",-10}\t{GetNthPercentile(sendTimes, 99)}"));
            await sw.WriteLineAsync(FormattableString.CurrentCulture($"{"99.9",-10}\t{GetNthPercentile(sendTimes, 99.9)}"));
            await sw.WriteLineAsync(FormattableString.CurrentCulture($"{"99.99",-10}\t{GetNthPercentile(sendTimes, 99.99)}"));
            await sw.WriteLineAsync(FormattableString.CurrentCulture($"{"100",-10}\t{GetNthPercentile(sendTimes, 100)}"));
            await sw.WriteLineAsync("");

            await sw.WriteLineAsync("Percentile\tTravel time");
            await sw.WriteLineAsync(FormattableString.CurrentCulture($"{"90",-10}\t{GetNthPercentile(travelTimes, 90)}"));
            await sw.WriteLineAsync(FormattableString.CurrentCulture($"{"95",-10}\t{GetNthPercentile(travelTimes, 95)}"));
            await sw.WriteLineAsync(FormattableString.CurrentCulture($"{"99",-10}\t{GetNthPercentile(travelTimes, 99)}"));
            await sw.WriteLineAsync(FormattableString.CurrentCulture($"{"99.9",-10}\t{GetNthPercentile(travelTimes, 99.9)}"));
            await sw.WriteLineAsync(FormattableString.CurrentCulture($"{"99.99",-10}\t{GetNthPercentile(travelTimes, 99.99)}"));
            await sw.WriteLineAsync(FormattableString.CurrentCulture($"{"100",-10}\t{GetNthPercentile(travelTimes, 100)}"));
            await sw.WriteLineAsync("");

            await sw.WriteLineAsync("Percentile\tTotal time");
            await sw.WriteLineAsync(FormattableString.CurrentCulture($"{"90",-10}\t{GetNthPercentile(totalTimes, 90)}"));
            await sw.WriteLineAsync(FormattableString.CurrentCulture($"{"95",-10}\t{GetNthPercentile(totalTimes, 95)}"));
            await sw.WriteLineAsync(FormattableString.CurrentCulture($"{"99",-10}\t{GetNthPercentile(totalTimes, 99)}"));
            await sw.WriteLineAsync(FormattableString.CurrentCulture($"{"99.9",-10}\t{GetNthPercentile(totalTimes, 99.9)}"));
            await sw.WriteLineAsync(FormattableString.CurrentCulture($"{"99.99",-10}\t{GetNthPercentile(totalTimes, 99.99)}"));
            await sw.WriteLineAsync(FormattableString.CurrentCulture($"{"100",-10}\t{GetNthPercentile(totalTimes, 100)}"));
            await sw.WriteLineAsync("");

            await sw.WriteLineAsync($"Average messages/second: {((double)numbers.Select(x => x.Value).Sum()/numbers.Count):##.##}");
            await sw.WriteLineAsync("");
            await sw.WriteLineAsync("Number of messages per second:");
            await sw.WriteLineAsync("");
            await sw.WriteLineAsync($"{"Time", -10}\tMessages/seg");
            foreach (var (key, value) in numbers)
                await sw.WriteLineAsync($"{key, -10}\t{value}");

            return $"benchmark_{compressEngine}.txt";
        }

        public void ClearDatabase()
        {
            _infosCollection.DeleteAll();
        }

        private static T GetNthPercentile<T>(IReadOnlyList<T> values, double percentile)
        {
            // calculating percentile position
            var percentilePosition = (int)Math.Ceiling(percentile / 100 * values.Count);
            if (percentilePosition >= values.Count) percentilePosition = values.Count - 1;
            return values[percentilePosition];
        }
    }
}
