namespace NanoMessageBus.DummyService.Handlers
{
    using System;
    using System.Threading.Tasks;
    using Messages;
    using Receiver.Services;
    using Repository;

    public class BigHandler : MessageHandlerBase<BigMessage>
    {
        private readonly Repository _repository;

        public BigHandler(Repository repository)
        {
            _repository = repository;
        }

        public override async Task RegisterStatisticsAsync(DateTime prepareToSendAt, DateTime sentAt, DateTime receivedAt, DateTime handledAt)
        {
            _repository.SaveTime((receivedAt-sentAt).TotalMilliseconds, (handledAt-prepareToSendAt).TotalMilliseconds);
            await Task.CompletedTask;
        }
    }
}
