namespace NanoMessageBus.BenchmarkService
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Compressor.DeflateJson;
    using Compressor.Protobuf;
    using DateTimeUtils;
    using Interfaces;
    using Messages;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using PropertyRetriever;
    using PropertyRetriever.Interfaces;
    using Receiver;
    using Repository;
    using Sender;

    public class Program
    {
        private static async Task Main()
        {
            int totalMessages, warmupMessages, parallel;
            string compress;
            var services = new ServiceCollection();

            // dependency injection
            services.AddPropertyRetriever();
            services.AddDateTimeUtils();
            services.AddLogging(c => c.ClearProviders());
            await using (var tempContainer = services.BuildServiceProvider())
            {
                parallel = tempContainer
                    .GetService<IPropertyRetriever>()
                    .RetrieveFromCommandLine("parallel", Environment.ProcessorCount).ToList()[0];
                totalMessages = tempContainer
                    .GetService<IPropertyRetriever>()
                    .RetrieveFromCommandLine("totalMessages", 1000000).ToList()[0];
                warmupMessages = tempContainer
                    .GetService<IPropertyRetriever>()
                    .RetrieveFromCommandLine("warmupMessages", 500).ToList()[0];

                compress = tempContainer.GetService<IPropertyRetriever>().RetrieveFromCommandLine("compress").ToList()[0];
                switch (compress)
                {
                    case "protobuf":
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Using protobuf compress engine");
                        services.AddNanoMessageBusProtobufCompressor();
                        break;
                    case "deflate":
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Using deflate json compress engine");
                        services.AddNanoMessageBusDeflateJsonCompressor();
                        break;
                    default:
                        compress = "default";
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Using default json compress engine");
                        break;
                }
            }
            services.AddSenderBus();
            services.AddReceiverBus();
            var countdown = new CountdownEvent(totalMessages);
            services.AddSingleton(countdown);
            services.AddSingleton<IBenchmarkRepository, BenchmarkRepository>();

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {warmupMessages} messages will be sent to load everything.");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {totalMessages} messages will be sent and benchmarked.");

            // creating service provider container with all dependency injections
            var container = services.BuildServiceProvider();
            container.UseSenderBus();
            container.UseReceiverBus();

            // start consuming messages
            container.ConsumeMessages();

            // start message sender
            var senderBus = container.GetSenderBus();

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Sending some messages to load everything...");
            Parallel.For(0, warmupMessages, new ParallelOptions { MaxDegreeOfParallelism = parallel }, async i =>
            {
                await senderBus.SendAsync(new Message
                {
                    Id = Guid.NewGuid(),
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
                    MessageContent19 = Guid.NewGuid().ToString(),
                    MessageContent20 = Guid.NewGuid().ToString(),
                    PersistMessage = false
                });
                Thread.Sleep(1);
            });

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Sending messages...");
            Parallel.For(0, totalMessages, new ParallelOptions { MaxDegreeOfParallelism = parallel }, async i =>
            {
                await senderBus.SendAsync(new Message
                {
                    Id = Guid.NewGuid(),
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
                    MessageContent19 = Guid.NewGuid().ToString(),
                    MessageContent20 = Guid.NewGuid().ToString(),
                    PersistMessage = true
                });
                Thread.Sleep(1);
            });
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Every message was sent!");
            countdown.Wait();

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Exporting results...");
            var exporedResultsFileName = await container.GetService<IBenchmarkRepository>().ExportFilteredDataAsync(totalMessages, compress);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Done!");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Results can be found on file {exporedResultsFileName}");
        }
    }
}
