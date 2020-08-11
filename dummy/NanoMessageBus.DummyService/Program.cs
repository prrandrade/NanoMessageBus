namespace NanoMessageBus.DummyService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Interfaces;
    using Compressor.DeflateJson;
    using Compressor.Protobuf;
    using DateTimeUtils;
    using Messages;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using PropertyRetriever;
    using PropertyRetriever.Interfaces;
    using Receiver;
    using Sender;

    public class Program
    {
        private static async Task Main()
        {
            var services = new ServiceCollection();

            // dependency injection
            services.AddPropertyRetriever();
            services.AddDateTimeUtils();
            services.AddLogging(c => c.ClearProviders());
            await using (var tempContainer = services.BuildServiceProvider())
            {
                var compress = tempContainer.GetService<IPropertyRetriever>().RetrieveFromCommandLine("compress").ToList()[0];
                switch (compress)
                {
                    case "protobuf":
                        Console.WriteLine("Using protobuf compress engine");
                        services.AddNanoMessageBusProtobufCompressor();
                        break;
                    case "deflate":
                        Console.WriteLine("using deflate json compress engine");
                        services.AddNanoMessageBusDeflateJsonCompressor();
                        break;
                    default:
                        Console.WriteLine("using default json compress engine");
                        break;
                }
            }
            services.AddSenderBus();
            services.AddReceiverBus();
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

            var sendType = container
                .GetService<IPropertyRetriever>()
                .RetrieveFromCommandLine("type", "mixed").ToList()[0];

            switch (sendType)
            {
                case "small":
                    Console.WriteLine("Preparing medium messages...");
                    var smallMessages = CreateABunchOfSmallMessages(totalMessages);
                    await senderBus.SendAsync(CreateSmallMessage());
                    await Task.Delay(1000);
                    Console.WriteLine("Sending small messages...");
                    Parallel.For(0, totalMessages, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, i =>
                    {
                        senderBus.SendAsync(smallMessages[i]);
                        Thread.Sleep(1);
                    });
                    break;
                case "medium":
                    Console.WriteLine("Preparing medium messages...");
                    var mediumMessages = CreateABunchOfMediumMessages(totalMessages);
                    await senderBus.SendAsync(CreateMediumMessage());
                    await Task.Delay(1000);
                    Console.WriteLine("Sending medium messages...");
                    Parallel.For(0, totalMessages, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, i =>
                    {
                        senderBus.SendAsync(mediumMessages[i]);
                        Thread.Sleep(1);
                    });
                    break;
                case "big":
                    Console.WriteLine("Preparing big messages...");
                    var bigMessages = CreateABunchOfBigMessages(totalMessages);
                    await senderBus.SendAsync(CreateBigMessage());
                    await Task.Delay(1000);
                    Console.WriteLine("Sending big messages...");
                    Parallel.For(0, totalMessages, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, i =>
                    {
                        senderBus.SendAsync(bigMessages[i]);
                        Thread.Sleep(1);
                    });
                    break;
                default:
                    Console.WriteLine("Preparing mixed messages...");
                    var messages = CreateABunchOfMessages(totalMessages);
                    await senderBus.SendAsync(CreateBigMessage());
                    await Task.Delay(1000);
                    Console.WriteLine("Sending mixed messages...");
                    Parallel.For(0, totalMessages, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, i =>
                    {
                        senderBus.SendAsync(messages[i]);
                        Thread.Sleep(1);
                    });
                    break;
            }

            Console.ReadKey(true);
            await container.GetService<Repository.Repository>().ExportTimeToCsv();
        }

        private static List<SmallMessage> CreateABunchOfSmallMessages(int quant)
        {
            var array = new SmallMessage[quant];
            Parallel.For(0, quant, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
            {
                array[i] = CreateSmallMessage();
            });
            return array.ToList();
        }

        private static List<MediumMessage> CreateABunchOfMediumMessages(int quant)
        {
            var array = new MediumMessage[quant];
            Parallel.For(0, quant, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
            {
                array[i] = CreateMediumMessage();
            });
            return array.ToList();
        }

        private static List<BigMessage> CreateABunchOfBigMessages(int quant)
        {
            var array = new BigMessage[quant];
            Parallel.For(0, quant, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
            {
                array[i] = CreateBigMessage();
            });
            return array.ToList();
        }

        private static List<IMessage> CreateABunchOfMessages(int quant)
        {
            var l = new List<IMessage>();
            for (var i = 0; i < quant; i++)
            {
                switch (i % 3)
                {
                    case 0:
                        l.Add(CreateSmallMessage());
                        break;
                    case 1:
                        l.Add(CreateMediumMessage());
                        break;
                    case 2:
                        l.Add(CreateBigMessage());
                        break;
                }
            }
            return l;
        }

        private static SmallMessage CreateSmallMessage()
        {
            return new SmallMessage
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
                MessageContent20 = Guid.NewGuid().ToString()
            };
        }

        private static MediumMessage CreateMediumMessage()
        {
            return new MediumMessage
            {
                Id = Guid.NewGuid(),
                Message01 = CreateSmallMessage(),
                Message02 = CreateSmallMessage(),
                Message03 = CreateSmallMessage(),
                Message04 = CreateSmallMessage(),
                Message05 = CreateSmallMessage()
            };
        }

        private static BigMessage CreateBigMessage()
        {
            return new BigMessage
            {
                Id = Guid.NewGuid(),
                Message1 = CreateMediumMessage(),
                Message2 = CreateMediumMessage(),
                Message3 = CreateMediumMessage()
            };
        }
    }
}
