namespace NanoMessageBus.Sender.Test.Models
{
    using Abstractions.Attributes;
    using Abstractions.Interfaces;

    internal class DummyIntMessage : IMessage
    {
        [MessageId]
        public int Id { get; set; }
    }
}
