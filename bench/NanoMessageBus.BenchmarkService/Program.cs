namespace NanoMessageBus.BenchmarkService
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Enums;
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
    using Serializers.DeflateJson;
    using Serializers.MessagePack;
    using Serializers.Protobuf;

    public class Program
    {
        private static async Task Main()
        {
            int totalMessages, warmupMessages, parallel;
            var services = new ServiceCollection();

            // dependency injection
            services.AddPropertyRetriever();
            services.AddDateTimeUtils();
            services.AddLogging(c => c.AddConsole());
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
            }

            services.AddNanoMessageBusProtobufSerialization();
            services.AddNanoMessageBusDeflateJsonSerialization();
            services.AddNanoMessageBusMessagePackSerialization();
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

            foreach (var serializationEngine in Enum.GetValues(typeof(SerializationEngine)).Cast<SerializationEngine>())
            {
                countdown.Reset();

                Console.WriteLine("");
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Now using {serializationEngine.ToString()}");
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
                    }, serializationEngine);
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
                    }, serializationEngine);
                    Thread.Sleep(1);
                });
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Every message was sent!");
                countdown.Wait();

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Exporting results...");
                var exporedResultsFileName = await container.GetService<IBenchmarkRepository>().ExportFilteredDataAsync(totalMessages, serializationEngine.ToString());
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Done!");
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Results can be found on file {exporedResultsFileName}");
                container.GetService<IBenchmarkRepository>().ClearDatabase();
            }
        }
    }
}
