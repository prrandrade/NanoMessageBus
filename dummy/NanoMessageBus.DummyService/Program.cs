namespace NanoMessageBus.DummyService
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
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

            Parallel.For(0, 100000, new ParallelOptions { MaxDegreeOfParallelism = 32 }, i =>
             {
                 var exampleMessage = new ExampleMessage
                 {
                     Id = i,
                     MessageContent = "Hello World!"
                 };
                 senderBus.SendAsync(exampleMessage);
                 Thread.Sleep(1);
             });

            Console.ReadKey(true);
        }
    }
}
