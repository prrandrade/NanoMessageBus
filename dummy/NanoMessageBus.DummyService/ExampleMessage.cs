namespace NanoMessageBus.DummyService
{
    using Abstractions.Attributes;
    using Abstractions.Interfaces;

    public class ExampleMessage : IMessage
    {
        [MessageId]
        public int Id { get; set; }

        public string MessageContent { get; set; }
    }
}
