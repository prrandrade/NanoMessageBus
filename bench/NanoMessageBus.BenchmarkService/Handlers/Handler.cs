namespace NanoMessageBus.BenchmarkService.Handlers
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;
    using Messages;
    using Receiver.Services;

    public class Handler : MessageHandlerBase<Message>
    {
        private readonly IBenchmarkRepository _repository;

        public Handler(IBenchmarkRepository repository)
        {
            _repository = repository;
        }

        public override async Task RegisterStatisticsAsync(DateTime prepareToSendAt, DateTime sentAt, DateTime receivedAt, DateTime handledAt)
        {
            _repository.SaveInfo(prepareToSendAt.ToBinary(), sentAt.ToBinary(), receivedAt.ToBinary(), handledAt.ToBinary());
            await Task.CompletedTask;
        }
    }
}
