namespace NanoMessageBus.Sender.Test.Models
{
    using System;
    using Abstractions.Attributes;
    using Abstractions.Interfaces;

    internal class DummyGuidMessage : IMessage
    {
        [MessageId]
        public Guid Id { get; set; }
    }
}
