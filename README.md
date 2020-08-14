# NanoMessageBus Sender & Receiver


# Summary

- [Introduction](#introduction)
- [Packages Installation with .NET Core Dependency Injection Framework](#package-installation-with-.net-core-dependency-injection-framework)
- [NanoMessageBus.Sender - How to send messages with RabbitMQ](#nanomessagebus.sender-how-to-send-messages-with-rabbitmq)
- [NanoMessageBus.Receiver - Receiving messages with RabbitMQ](#nanomessagebus.receiver-receiving-messages-with-rabbitmq)
- [Benchmark scenario](#benchmark-scenario)
- [Customized Serializers](#customized-serializers)

# Introduction

The NanoMessageBus is built with two packages - **NanoMessaegBus.Sender** and **NanoMessageBus.Receiver** with a simple objective: abstract all the logic of a [RabbitMQ server](https://www.rabbitmq.com/) communication and send/receive asynchronous messages in an **easy** and **fast** way!



# Packages installation with .NET Core Dependency Injection Framework

Using the native .NET Core dependency injection framework, you can install the **NanoMessageBus.Sender** package and the **NanoMessageBus.Receiver** package separately. Using a default `ServiceColletion` object, you can use the extension methods `AddSenderBus` and `AddReceiverBus` which register all the related dependencies.

```csharp
var services = new ServiceCollection();
services.AddSenderBus(); // for NanoMessageBus.Sender
services.AddReceiverBus(); // for NanoMessageBus.Receiver
```

With this, you can inject a singleton `ISenderBus` to send messages and a singleton `IReceiverBus` to receive messages. Both can be loaded with the `ServiceProvider` extensions methods. The RabbitMQ connections, with related exchanges and queues, are created at this moment:

```csharp
var container = services.BuildServiceProvider();
container.UseSenderBus(); // connections, exchanges and queues for sending messages are created here!
container.UseReceiverBus(); // connections, exchanges and queues for receiving messages are created here!
```

Last but not least, when you are using the **NanoMessageBus.Receiver** to receive messages, you must use the extension method `ConsumeMessages` to start receiving messages.

```csharp
container.ConsumeMessages();
```



# NanoMessageBus.Sender - How to send messages with RabbitMQ

The following parameters must be set when you have a service that will send RabbitMQ messages:

- **brokerHostname**: RabbitMQ hostname with a port. Can be an environment variable or a command-line argument. The default value is **localhost:5672**. When using a cluster environment, you can pass all the hostnames using a CSV list.
- **brokerVirtualHost**: RabbitMQ virtual host. Can be an environment variable or a command-line argument. The default value is **/**.
- **brokerUsername**: RabbitMQ username. Can be an environment variable or a command-line argument. The default value is **guest**.
- **brokerPassword**: RabbitMQ password. Can be an environment variable or a command-line argument. The default value is **guest**.
- **brokerIdentification**: How this service will be internally and externally identified. Can be an environment variable or a command-line argument. The default value is **NoIdentification**, but is ***absolutely recommended*** that each of your services has individual identification.

- **brokerMaxShardingSize**: How many shards the whole system will have. This property must be the same value for all services, that's why it is only an environment variable. The default value is **10**.

When a service with the **NanoMessageBus.Sender** package is started, you can see that some RabbitMQ Exchanges are created - the same quantity of shards (property **brokerMaxShardingSize**), and identified with the property **brokerIdentification**. For example, for a service identified as _ExampleService_ and with 10 shards, the following Rabbit MQ exchanges are created:

- exchange.ExampleService.0
- exchange.ExampleService.1
- exchange.ExampleService.2
-  exchange.ExampleService.3
-  exchange.ExampleService.4
-  exchange.ExampleService.5
-  exchange.ExampleService.6
-  exchange.ExampleService.7
-  exchange.ExampleService.8
-  exchange.ExampleService.9

These exchanges are fanout exchanges - every message that an exchange receives is passed to every queue registered - in other words, every message sent with the **NanoMessageBus.Sender** is **broadcasted**, so never sent a message to a specific service. So, how the SenderBus defined which exchanges received which message?

Every sent message must implement the interface `IMessage` and must have one `Int` or `Guid` parameter. with the attribute `[MessageId]`.  This attribute identify which _shard_, or in this case, _exchange_,  will be used to send the message.

Let's use the `ExampleMessage` class:

```csharp
public class ExampleMessage : IMessage
{
    [MessageId]
    public int Id { get; set; }
}
```

And let's send a message:

```csharp
public async Task Example(ISenderBus senderBus) {
	var message = new Message { Id = 1 };
    await senderBus.SendAsync(message);
}
```

With the default approach, the message will be sent to _exchange.ExampleService.1_. The default _shardResolver_ function for integer values is a simple module operation with the maximum number of shards (remember, property **brokerMaxShardingSize**). In this case, messages with **Id 11**, for example, will also be sent to _exchange.ExampleService.1_.

Now let's use the `ExampleOtherMessage` class:

```csharp
public class ExampleOtherMessage : IMessage
{
    [MessageId]
    public Guid Id { get; set; }
}
```

And let's send a message:

```csharp
public async Task Example(ISenderBus senderBus) {
	var message = new Message { Id = Guid.Parse("00000000-0000-0000-0000-000000000000") };
    await senderBus.SendAsync(message);
}
```

With the default approach, the message will be sent to _exchange.ExampleService.0_. The default _shardResolver_ for Guid values is to obtain the sum of all bytes inside a `Guid` (16 bytes) and apply a module operation with the maximum number of shards (again, property **brokerMaxShardingSize**).

You can create a customized _shardResolver_ function, with needs to be a `Func<object, int, int>`. For example:

```csharp
public async Task Example(ISenderBus senderBus) {    
    Func<object, int, int> customShard = (obj, maxShard) => ((int)obj+1) % maxShard;
	var message = new Message { Id = 1 };
    await senderBus.SendAsync(message, shardResolver: customShard);
}
```

You can also change the priority of the sent message, using the enumeration `MessagePriority`. This enumeration uses the default RabbitMQ priority system.

```csharp
await senderBus.SendAsync(message, MessagePriority.NormalPriority); // that's the default parameter
await senderBus.SendAsync(message, MessagePriority.Level1Priority);
await senderBus.SendAsync(message, MessagePriority.Level2Priority);
await senderBus.SendAsync(message, MessagePriority.Level3Priority);
await senderBus.SendAsync(message, MessagePriority.Level4Priority);
```

Every message is serialized using the default `Systme.Text.Json` .Net Core package and broadcasted to its respective exchange.

As an example, the _ExampleService_ can be started like this (note that all other properties are using the default values):

```powershell
./Service.exe --brokerIdentification ExampleService
```



# NanoMessageBus.Receiver - Receiving messages with RabbitMQ

The following parameters must be set when you have a service that will receive RabbitMQ messages:

- **brokerHostname**: RabbitMQ hostname with a port. Can be an environment variable or a command-line argument. The default value is **localhost:5672**. When using a cluster environment, you can pass all the hostnames using a CSV list.
- **brokerVirtualHost**: RabbitMQ virtual host. Can be an environment variable or a command-line argument. The default value is **/**.
- **brokerUsername**: RabbitMQ username. Can be an environment variable or a command-line argument. The default value is **guest**.
- **brokerPassword**: RabbitMQ password. Can be an environment variable or a command-line argument. The default value is **guest**.
- **brokerIdentification**: How this service will be internally and externally identified. Can be an environment variable or a command-line argument. The default value is **NoIdentification**, but is ***absolutely recommended*** that each of your services has individual identification.

- **brokerMaxShardingSize**: How many shards the whole system will have. This property must be the same value for all services, that's why it is only an environment variable. The default value is **1**.
- **brokerListenedServices**: CSV list of which *services* will be listened. Can be an environment variable or a command-line argument. The default value is the same **brokerIdentification** (service will listen to its messages!).
- **brokerListenedShards**: CSV list of which _shards_ will be listened. Can be an environment variable or a command-line argument. The default value is **0-brokerMaxShardingSize** - all shards available.

So first and foremost, the service with the **NanoMessageBus.Receiver** package will create queues to receive messages sent to the exchanges created by the listened services. Let's example it:

- You have a service with the identifier *ReceiverService* that will listen for sent messages of *SenderService* and *AnotherSenderService*, but listening only shards **1, 2, 4, 5, 6, 7**. 
- You can pass **ExampleService** with the **brokerListenedServices** parameter for *ReceiverService*.
- You can pass **1,2,4-7** with the **brokerListenedShards** parameter for *ReceiverService*.

With these configurations, the *ReceiverService* will create the following queues:

- **queue.ReceiverService.1**, receiving all messages from **exchange.SenderService.1** and all messages from **exchange.AnotherSenderService.1**.
- **queue.ReceiverService.2**, receiving all messages from **exchange.SenderService.2** and all messages from **exchange.AnotherSenderService.2**.
- **queue.ReceiverService.4**, receiving all messages from **exchange.SenderService.4** and all messages from **exchange.AnotherSenderService.4**.
- **queue.ReceiverService.5**, receiving all messages from **exchange.SenderService.5** and all messages from **exchange.AnotherSenderService.5**.
- **queue.ReceiverService.6**, receiving all messages from **exchange.SenderService.6** and all messages from **exchange.AnotherSenderService.6**.
- **queue.ReceiverService.7**, receiving all messages from **exchange.SenderService.7** and all messages from **exchange.AnotherSenderService.7**.

With this approach, you can create more shards of the same service with simple configuration changes, for better messaging throughput.

The messages are always processed using message handlers, classes that implement the abstract class `MessageHandlerBase<>`. For example, if you want to receive this message:

```csharp
public class ExampleMessage : IMessage
{
    [MessageId]
    public int Id { get; set; }
}
```

Then you need to implement a `MessageHandlerBase<ExampleMessage>`. The **NanoMessageBus.Receiver** handles with dependency injection, using a [`IServiceScopeFactory`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicescopefactory?view=dotnet-plat-ext-3.1).

```csharp
public class ExampleHandler : MessageHandlerBase<ExampleMessage> { }
```

- There are for methods inside `MessageHandlerBase<>` that can be overridden to process the received message:
- `Task RegisterStatisticsAsync(TMessage message, int messageSize, DateTime prepareToSendAt, DateTime sentAt, DateTime receivedAt, DateTime handledAt)`, for statistics purpose, this method will receive the UTC DateTimes to mark when the message was sent and received.
- `Task<bool> BeforeHandleAsync(TMessage message)`, if you want to check if this message can be processed.
- `Task HandleAsync(TMessage message)`, to, in fact, process the message.
- `Task AfterHandleAsync(TMessage message)`, to apply some post-processing for the received message.

As today, every message is [auto acked](https://www.rabbitmq.com/confirms.html) and a received message can never generate an answer to the service that sent it - remember, every message is **broadcasted ** in **an asynchronous one-way path**.

As an example, the _ReceiverService_ can be started like this (note that all other properties are using the default values):

```powershell
./Service.exe --brokerIdentification ReceiverService --brokerListenedServices SenderService,AnotherSenderService
```



# Benchmark scenario

I've run a benchmark scenario using both of my machines as a client (with the service) and as a server (with a RabbitMQ Docker container). 500000 messages were sent in batches of 16 messages with 1 ms of interval. 

- My **Client** is a Intel Core i3-9100T (4 cores and 4 threads) with 8GB RAM.
- My **Rabbit MQ Server** is a AMD Ryzen 9 3950x (16 cores and 32 threads) with 64GB RAM.

On average **595 messages** were sent **per second**, each message with **1227 bytes**, and that's the results in milliseconds:

|Percentile|Total time (ms)|
| ---- | ---- |
| 90                | 13.105          |
| 95                | 99              |
| 99                | 30.165          |
| 99.9              | 94.535          |
| 99.99             | 990.832         |
| 100 (worse case!) | 3293.314        |

As you can see, more than **99%** of **500000 messages** were processed on less than **100 ms**, using a Wi-Fi network!



# Customized Serializers

These benchmark numbers can be achived using the default JSON serializer used by NanoMessageBus.Sender and NanoMessageBus.Receiver. You can, however, use customized serializers for more network performance or more serialization performance. As today, we have three cutomized serializers:

- NanoMessageBus.Serializers.DeflateJson, [using DeflateStream](https://docs.microsoft.com/pt-br/dotnet/api/system.io.compression.deflatestream?view=netcore-3.1) to compress the serialized Json.
- NanoMessageBus.Serializers.Protobuf, [using Protobuf-net](https://github.com/protobuf-net/protobuf-net) for serialization.
- NanoMessageBus.Serializers.MessagePack, [using MessagePack](https://github.com/neuecc/MessagePack-CSharp)  for serialization.

The installation is simple, but **only one package must be used**:

```csharp
var services = new ServiceCollection();
services.AddNanoMessageBusDeflateJsonSerialization(); // now the packages will serialize/deserialize all the messafges using the NanoMessageBus.Serializers.DeflateJson package
```

```csharp
var services = new ServiceCollection();
services.AddNanoMessageBusProtobufSerialization(); // now the packages will serialize/deserialize all the messafges using the NanoMessageBus.Serializers.Protobuf package
```

```csharp
var services = new ServiceCollection();
services.AddNanoMessageBusMessagePackSerialization(); // now the packages will serialize/deserialize all the messafges using the NanoMessageBus.Serializers.MessagePack package
```



The performance is also different, because you're adding more processing for serialization and deserialization, but a small message results in more messages being sent per second. Each scenario deserves a particular test, but in my particular scenario, the results are interesting:



#### NanoMessageBus.Serializers.DeflateJson

On average **653 messages** were sent **per second**, each message with **578 bytes**, and that's the results in milliseconds:

| Percentile        | Total time (ms) |
| ----------------- | --------------- |
| 90                | 11.520          |
| 95                | 13.244          |
| 99                | 21.999          |
| 99.9              | 599.229         |
| 99.99             | 1000.898        |
| 100 (worse case!) | 2770.285        |

 #### NanoMessageBus.Serializers.Protobuf

On average **654 messages** were sent **per second**, each message with **789 bytes**, and that's the results in milliseconds:

| Percentile        | Total time (ms) |
| ----------------- | --------------- |
| 90                | 11.470          |
| 95                | 13.262          |
| 99                | 22.917          |
| 99.9              | 773.379         |
| 99.99             | 985.780         |
| 100 (worse case!) | 1025.408        |

#### NanoMessageBus.Serializers.MessagePack

On average **493 messages** were sent **per second**, each message with **1140 bytes**, and that's the results in milliseconds:

| Percentile        | Total time (ms) |
| ----------------- | --------------- |
| 90                | 18.391          |
| 95                | 23.396          |
| 99                | 46.503          |
| 99.9              | 661.626         |
| 99.99             | 1030.943        |
| 100 (worse case!) | 5951.829        |