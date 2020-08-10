namespace NanoMessageBus.Sender.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Abstractions.Enums;
    using Abstractions.Interfaces;

    public interface ISenderBus
    {
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
        /// <param name="priority">Message priority. Messages with more priority are processed earlier.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SendAsync(IMessage message, MessagePriority priority);

        /// <summary>
        /// Send a message via RabbitMQ to all listening services.
        /// </summary>
        /// <param name="message">Message that will be sent.</param>
        /// <param name="shardResolver">Customized function to decide which shard will be used. The first parameter must be converted to Guid or Int.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SendAsync(IMessage message, Func<object, int, int> shardResolver);

        /// <summary>
        /// Send a message via RabbitMQ to all listening services.
        /// </summary>
        /// <param name="message">Message that will be sent.</param>
        /// <param name="priority">Message priority. Messages with more priority are processed earlier.</param>
        /// <param name="shardResolver">Customized function to decide which shard will be used. The first parameter must be converted to Guid or Int.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SendAsync(IMessage message, MessagePriority priority, Func<object, int, int> shardResolver);
    }
}
