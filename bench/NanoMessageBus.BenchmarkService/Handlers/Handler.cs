namespace NanoMessageBus.BenchmarkService.Handlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Interfaces;
    using Messages;
    using Receiver.Services;

    public class Handler : MessageHandlerBase<Message>
    {
        private readonly IBenchmarkRepository _repository;
        private readonly CountdownEvent _countdown;

        public Handler(IBenchmarkRepository repository, CountdownEvent countdown)
        {
            _repository = repository;
            _countdown = countdown;
        }

        public override async Task RegisterStatisticsAsync(Message message, int messageSize, DateTime prepareToSendAt, DateTime sentAt, DateTime receivedAt, DateTime handledAt)
        {
            if (message.PersistMessage)
            {
                _repository.SaveInfo(message.Id, messageSize, prepareToSendAt.ToBinary(), sentAt.ToBinary(), receivedAt.ToBinary(), handledAt.ToBinary());
                _countdown.Signal();
            }
            await Task.CompletedTask;
        }
    }
}
