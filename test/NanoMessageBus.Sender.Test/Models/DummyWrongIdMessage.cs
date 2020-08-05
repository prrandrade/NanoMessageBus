namespace NanoMessageBus.Sender.Test.Models
{
    using Abstractions.Attributes;
    using Abstractions.Interfaces;

    internal class DummyWrongIdMessage : IMessage
    {
        [MessageId]
        public double Id { get; set; }
    }
}
