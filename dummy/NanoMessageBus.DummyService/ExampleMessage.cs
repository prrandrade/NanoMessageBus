namespace NanoMessageBus.DummyService
{
    using System;
    using Abstractions.Attributes;
    using Abstractions.Interfaces;

    public class ExampleMessage : IMessage
    {
        [MessageId]
        public Guid Id { get; set; }

        public string MessageContent00 { get; set; }

        public string MessageContent01 { get; set; }

        public string MessageContent02 { get; set; }

        public string MessageContent03 { get; set; }

        public string MessageContent04 { get; set; }

        public string MessageContent05 { get; set; }

        public string MessageContent06 { get; set; }

        public string MessageContent07 { get; set; }

        public string MessageContent08 { get; set; }

        public string MessageContent09 { get; set; }

        public string MessageContent10 { get; set; }

        public string MessageContent11 { get; set; }

        public string MessageContent12 { get; set; }

        public string MessageContent13 { get; set; }

        public string MessageContent14 { get; set; }

        public string MessageContent15 { get; set; }

        public string MessageContent16 { get; set; }

        public string MessageContent17 { get; set; }

        public string MessageContent18 { get; set; }

        public string MessageContent19 { get; set; }
    }
}
