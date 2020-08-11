namespace NanoMessageBus.DummyService.Messages
{
    using System;
    using Abstractions.Attributes;
    using Abstractions.Interfaces;
    using ProtoBuf;

    [ProtoContract]
    public class MediumMessage : IMessage
    {
        [ProtoMember(1)]
        [MessageId]
        public Guid Id { get; set; }

        [ProtoMember(2)]
        public SmallMessage Message01 { get; set; }

        [ProtoMember(3)]
        public SmallMessage Message02 { get; set; }

        [ProtoMember(4)]
        public SmallMessage Message03 { get; set; }

        [ProtoMember(5)]
        public SmallMessage Message04 { get; set; }

        [ProtoMember(6)]
        public SmallMessage Message05 { get; set; }
    }
}
