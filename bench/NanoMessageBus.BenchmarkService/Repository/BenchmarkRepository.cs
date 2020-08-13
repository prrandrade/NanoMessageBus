namespace NanoMessageBus.BenchmarkService.Repository
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
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

        public async Task ExportDataAsync()
        {
            await using var sw = new StreamWriter("outputInfos.txt", false) {AutoFlush = true};
            await sw.WriteLineAsync("Start Time;Message Size (bytes);Send Time;Travel Time;Total Time");
            var infos = _infosCollection.FindAll().ToList();

            for (var i = 0; i < infos.Count; i++)
            {
                Console.WriteLine($"Writing message {i+1} of {infos.Count}");
                var info = infos[i];
                await sw.WriteLineAsync(FormattableString.CurrentCulture($"{DateTime.FromBinary(info.PrepareToSendAt):HH:mm:ss.fff};{info.MessageSize};{info.SendTime};{info.TravelTime};{info.TotalTime}"));
            }

            
        }
    }
}
