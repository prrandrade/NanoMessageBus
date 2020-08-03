namespace NanoMessageBus.DummyService
{
    using System;
    using DateTimeUtils;
    using Microsoft.Extensions.DependencyInjection;
    using PropertyRetriever;
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

            // creating service provider container with all dependency injections
            var container = services.BuildServiceProvider();

            // start message sender
            var senderBus = container.GetSenderBus();

            var exampleMessage = new ExampleMessage
            {
                Id = 15,
                MessageContent = "Hello World!"
            };

            senderBus.SendAsync(exampleMessage);
        }
    }
}
