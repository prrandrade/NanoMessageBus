namespace NanoMessageBus.Receiver.Test.Models
{
    using System;
    using Abstractions.Attributes;
    using Abstractions.Interfaces;

    public class DummyGuidMessage : IMessage
    {
        [MessageId]
        public Guid Id { get; set; }
    }
}
