namespace NanoMessageBus.DummyService
{
    using System;
    using DateTimeUtils;
    using Microsoft.Extensions.DependencyInjection;
    using PropertyRetriever;
    using Receiver;
    using Sender;

    public class Program
    {
        private static void Main()
        {
            var services = new ServiceCollection();

            // dependency injection
            services.AddPropertyRetriever();
            services.AddDateTimeUtils();
            services.AddSenderBus();
            services.AddReceiverBus();

            // creating service provider container with all dependency injections
            var container = services
                .BuildServiceProvider()
                .LoadSenderBus()
                .LoadReceiverBus();

            // start consuming messages
            container.ConsumeMessages();

            // start message sender
            var senderBus = container.GetSenderBus();

            for (var i = 0; i < 10000; i++)
            {
                var exampleMessage = new ExampleMessage
                {
                    Id = i,
                    MessageContent = "Hello World!"
                };
                senderBus.SendAsync(exampleMessage);
            }

            Console.ReadKey(true);
        }
    }
}
