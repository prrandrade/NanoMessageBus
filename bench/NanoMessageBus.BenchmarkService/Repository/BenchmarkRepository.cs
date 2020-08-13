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
        private readonly CountdownEvent _countdown;
        private readonly ILiteCollection<InfoModel> _infosCollection;

        public BenchmarkRepository(CountdownEvent countdown)
        {
            _countdown = countdown;
            var db = new LiteDatabase("database.db");
            _infosCollection = db.GetCollection<InfoModel>("infos");
            _infosCollection.DeleteAll();
        }

        public void SaveInfo(long prepareToSentAt, long sentAt, long receivedAt, long handledAt)
        {
            _infosCollection.Insert(new InfoModel
            {
                PrepareToSendAt = prepareToSentAt,
                SentAt = sentAt,
                ReceivedAt = receivedAt,
                HandledAt = handledAt
            });
            _countdown.Signal();
        }

        public async Task ExportDataAsync()
        {
            await using var sw = new StreamWriter("outputInfos.txt", false) {AutoFlush = true};
            await sw.WriteLineAsync("Start Time;Send Time;Travel Time;Total Time");
            var infos = _infosCollection.FindAll().ToList();
            foreach (var info in infos)
                await sw.WriteLineAsync(FormattableString.CurrentCulture($"{DateTime.FromBinary(info.PrepareToSendAt):HH:mm:ss.fff};{info.SendTime};{info.TravelTime};{info.TotalTime}"));
        }
    }
}
