namespace NanoMessageBus.BenchmarkService.Messages
{
    using System;
    using Abstractions.Attributes;
    using Abstractions.Interfaces;
    using ProtoBuf;

    [ProtoContract]
    public class Message : IMessage
    {
        [MessageId]
        [ProtoMember(1)]
        public Guid Id { get; set; }

        [ProtoMember(2)]
        public string MessageContent01 { get; set; }

        [ProtoMember(3)]
        public string MessageContent02 { get; set; }

        [ProtoMember(4)]
        public string MessageContent03 { get; set; }

        [ProtoMember(5)]
        public string MessageContent04 { get; set; }

        [ProtoMember(6)]
        public string MessageContent05 { get; set; }

        [ProtoMember(7)]
        public string MessageContent06 { get; set; }

        [ProtoMember(8)]
        public string MessageContent07 { get; set; }

        [ProtoMember(9)]
        public string MessageContent08 { get; set; }

        [ProtoMember(10)]
        public string MessageContent09 { get; set; }

        [ProtoMember(11)]
        public string MessageContent10 { get; set; }

        [ProtoMember(12)]
        public string MessageContent11 { get; set; }

        [ProtoMember(13)]
        public string MessageContent12 { get; set; }

        [ProtoMember(14)]
        public string MessageContent13 { get; set; }

        [ProtoMember(15)]
        public string MessageContent14 { get; set; }

        [ProtoMember(16)]
        public string MessageContent15 { get; set; }

        [ProtoMember(17)]
        public string MessageContent16 { get; set; }

        [ProtoMember(18)]
        public string MessageContent17 { get; set; }

        [ProtoMember(19)]
        public string MessageContent18 { get; set; }

        [ProtoMember(20)]
        public string MessageContent19 { get; set; }

        [ProtoMember(21)]
        public string MessageContent20 { get; set; }

        [ProtoMember(22)]
        public bool PersistMessage { get; set; }
    }
}
