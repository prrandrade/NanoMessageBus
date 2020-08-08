namespace NanoMessageBus.DummyService.Repository
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using LiteDB;
    using Models;

    public class Repository
    {
        private readonly ILiteCollection<TimeModel> _collection;

        public Repository()
        {
            var db = new LiteDatabase("database.db");
            _collection = db.GetCollection<TimeModel>("times");
            _collection.DeleteAll();
        }

        public void SaveTime(double travelTime, double totalTime)
        {
            _collection.Insert(new TimeModel
            {
                TravelTime = travelTime,
                TotalTime = totalTime
            });
        }

        public async Task ExportTimeToCsv()
        {
            await using var sw = new StreamWriter("output.txt", false) {AutoFlush = true};
            await sw.WriteLineAsync("travel time;total time");
            var times = _collection.FindAll().ToList();
            foreach (var time in times)
                await sw.WriteLineAsync(FormattableString.CurrentCulture($"{time.TravelTime};{time.TotalTime}"));
        }
    }
}
