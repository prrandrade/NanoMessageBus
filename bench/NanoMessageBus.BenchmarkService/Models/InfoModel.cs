namespace NanoMessageBus.BenchmarkService.Models
{
    using System;

    public class InfoModel
    {
        public int Id { get; set; }

        public Guid MessageId { get; set; }

        public long PrepareToSendAt { get; set; }

        public long SentAt { get; set; }

        public long ReceivedAt { get; set; }

        public long HandledAt { get; set; }

        public double SendTime => (DateTime.FromBinary(SentAt) - DateTime.FromBinary(PrepareToSendAt)).TotalMilliseconds;
         
        public double TravelTime => (DateTime.FromBinary(ReceivedAt) - DateTime.FromBinary(SentAt)).TotalMilliseconds;

        public double TotalTime => (DateTime.FromBinary(HandledAt) - DateTime.FromBinary(PrepareToSendAt)).TotalMilliseconds;


    }
}
