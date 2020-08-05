namespace NanoMessageBus.Receiver.Test.Models
{
    using Abstractions.Attributes;
    using Abstractions.Interfaces;

    public class DummyIntMessage : IMessage
    {
        [MessageId]
        public int Id { get; set; }
    }
}
