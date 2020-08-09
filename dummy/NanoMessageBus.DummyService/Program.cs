namespace NanoMessageBus.DummyService
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DateTimeUtils;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using PropertyRetriever;
    using PropertyRetriever.Interfaces;
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
            services.AddLogging(c => c.ClearProviders());
            services.AddSingleton<Repository.Repository>();

            // creating service provider container with all dependency injections
            var container = services.BuildServiceProvider();
            container.UseSenderBus();
            container.UseReceiverBus();

            // start consuming messages
            container.ConsumeMessages();

            // start message sender
            var senderBus = container.GetSenderBus();

            var totalMessages = container
                .GetService<IPropertyRetriever>()
                .RetrieveFromCommandLine("totalMessages", 1000000).ToList()[0];

            var maxDegreeOfParallelism = container
                .GetService<IPropertyRetriever>()
                .RetrieveFromCommandLine("parallel", Environment.ProcessorCount).ToList()[0];

            Parallel.For(0, totalMessages, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, i =>
            {
                var exampleMessage = new ExampleMessage
                {
                    Id = Guid.NewGuid(),
                    MessageContent00 = Guid.NewGuid().ToString(),
                    MessageContent01 = Guid.NewGuid().ToString(),
                    MessageContent02 = Guid.NewGuid().ToString(),
                    MessageContent03 = Guid.NewGuid().ToString(),
                    MessageContent04 = Guid.NewGuid().ToString(),
                    MessageContent05 = Guid.NewGuid().ToString(),
                    MessageContent06 = Guid.NewGuid().ToString(),
                    MessageContent07 = Guid.NewGuid().ToString(),
                    MessageContent08 = Guid.NewGuid().ToString(),
                    MessageContent09 = Guid.NewGuid().ToString(),
                    MessageContent10 = Guid.NewGuid().ToString(),
                    MessageContent11 = Guid.NewGuid().ToString(),
                    MessageContent12 = Guid.NewGuid().ToString(),
                    MessageContent13 = Guid.NewGuid().ToString(),
                    MessageContent14 = Guid.NewGuid().ToString(),
                    MessageContent15 = Guid.NewGuid().ToString(),
                    MessageContent16 = Guid.NewGuid().ToString(),
                    MessageContent17 = Guid.NewGuid().ToString(),
                    MessageContent18 = Guid.NewGuid().ToString(),
                    MessageContent19 = Guid.NewGuid().ToString()
                };
                senderBus.SendAsync(exampleMessage);
                Thread.Sleep(1);
            });

            Console.ReadKey(true);
            container.GetService<Repository.Repository>().ExportTimeToCsv().Wait();
        }
    }
}
