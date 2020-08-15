namespace NanoMessageBus.Sender.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Abstractions.Enums;
    using Abstractions.Interfaces;
    using EventArgs;

    public interface ISenderBus
    {
        /// <summary>
        /// Event triggered when a Message is sent
        /// </summary>
        public event EventHandler<MessageSentEventArgs> MessageSent;

        /// <summary>
        /// Send a message via RabbitMQ to all listening services.
        /// </summary>
        /// <param name="message">Message that will be sent.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SendAsync(IMessage message);

        /// <summary>
        /// Send a message via RabbitMQ to all listening services.
        /// </summary>
        /// <param name="message">Message that will be sent.</param>
        /// <param name="serializationEngine">Serialization engine used to serialize this message (the listening services MUST have the serialization engine installed!)</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SendAsync(IMessage message, SerializationEngine serializationEngine);

        /// <summary>
        /// Send a message via RabbitMQ to all listening services.
        /// </summary>
        /// <param name="message">Message that will be sent.</param>
        /// <param name="serializationEngine">Serialization engine used to serialize this message (the listening services MUST have the serialization engine installed!)</param>
        /// <param name="priority">Message priority. Messages with more priority are processed earlier.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SendAsync(IMessage message, SerializationEngine serializationEngine, MessagePriority priority);

        /// <summary>
        /// Send a message via RabbitMQ to all listening services.
        /// </summary>
        /// <param name="message">Message that will be sent.</param>
        /// <param name="serializationEngine">Serialization engine used to serialize this message (the listening services MUST have the serialization engine installed!)</param>
        /// <param name="shardResolver">Customized function to decide which shard will be used. The first parameter must be converted to Guid or Int.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SendAsync(IMessage message, SerializationEngine serializationEngine, Func<object, int, int> shardResolver);

        /// <summary>
        /// Send a message via RabbitMQ to all listening services.
        /// </summary>
        /// <param name="message">Message that will be sent.</param>
        /// <param name="serializationEngine">Serialization engine used to serialize this message (the listening services MUST have the serialization engine installed!)</param>
        /// <param name="priority">Message priority. Messages with more priority are processed earlier.</param>
        /// <param name="shardResolver">Customized function to decide which shard will be used. The first parameter must be converted to Guid or Int.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SendAsync(IMessage message, SerializationEngine serializationEngine, MessagePriority priority, Func<object, int, int> shardResolver);
    }
}
