namespace NanoMessageBus.DummyService
{
    using Abstractions;

    public class ExampleMessage : IMessage
    {
        [MessageId]
        public int Id { get; set; }

        public string MessageContent { get; set; }
    }
}
