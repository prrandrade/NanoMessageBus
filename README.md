# NanoMessageBus Sender & Receiver


# Summary

- [Introduction](#introduction)
- [Packages Installation with .NET Core Dependency Injection Framework](#package-installation-with-.net-core-dependency-injection-framework)
- [Manual Installation with other Dependency Injection Frameworks](#manual-installation-with-other-dependency-injection-frameworks)
- [NanoMessageBus.Sender - How to send messages with RabbitMQ](#nanomessagebus.sender-how-to-send-messages-with-rabbitmq)
- [NanoMessageBus.Receiver - Receiving messages with RabbitMQ](#nanomessagebus.receiver-receiving-messages-with-rabbitmq)

- [Benchmark scenario](#benchmark-scenario)

# Introduction

The NanoMessageBus is built with two packages - **NanoMessaegBus.Sender** and **NanoMessageBus.Receiver** with a simple objective: abstract all the logic of a [Rabbit MQ server](https://www.rabbitmq.com/) communication and send/receive asynchronous messages in an **easy** and **fast** way!



# Packages installation with .NET Core Dependency Injection Framework

Using the native .NET Core dependency injection framework, you can install the **NanoMessageBus.Sender** and the **NanoMessageBus.Receiver** separately. Using a default `ServiceColletion` object, you can use the extension methods `AddSenderBus` and `AddReceiverBus`, which register all the related dependencies.

```csharp
var services = new ServiceCollection();
services.AddSenderBus(); // for NanoMessageBus.Sender
services.AddReceiverBus(); // for NanoMessageBus.Receiver
```

With this, you can inject a singleton `ISenderBus` to send messages and a singleton `IReceiverBus` to receive messages. Both can be loaded with `ServiceProvider` extensions methods. The RabbitMQ connections, with related exchanges and queues, are created at this moment:

```csharp
var container = services.BuildServiceProvider();
container.UseSenderBus(); // connections, exchanges and queues for sending messages are created here!
container.UseReceiverBus(); // connections, exchanges and queues for receiving messages are created here!
```

Last but not least, when you are using the **NanoMessageBus.Receiver** to receive messages, you must use the extension method `ConsumeMessages` to actually start receiving messages.

```csharp
container.ConsumeMessages();
```



