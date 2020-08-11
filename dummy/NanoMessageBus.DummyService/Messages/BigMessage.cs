namespace NanoMessageBus.DummyService.Messages
{
    using System;
    using Abstractions.Attributes;
    using Abstractions.Interfaces;
    using ProtoBuf;

    [ProtoContract]
    public class BigMessage : IMessage
    {
        [ProtoMember(1)]
        [MessageId]
        public Guid Id {get; set; }

        [ProtoMember(2)]
        public MediumMessage Message1 { get; set; }

        [ProtoMember(3)]
        public MediumMessage Message2 { get; set; }

        [ProtoMember(4)]
        public MediumMessage Message3 { get; set; }
    }
}
