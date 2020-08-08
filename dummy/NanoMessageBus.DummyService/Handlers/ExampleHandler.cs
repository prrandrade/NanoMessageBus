namespace NanoMessageBus.DummyService.Handlers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Receiver.Services;
    using Repository;

    public class ExampleHandler : MessageHandlerBase<ExampleMessage>
    {
        private readonly ILogger<ExampleHandler> _logger;
        private readonly Repository _repository;

        public ExampleHandler(ILogger<ExampleHandler> logger, Repository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public override async Task RegisterStatisticsAsync(long prepareToSendAt, long sentAt, long receivedAt, long handledAt)
        {
            var prepareToSendDate = DateTime.FromBinary(prepareToSendAt);
            var sentAtDate = DateTime.FromBinary(sentAt);
            var receivedAtDate = DateTime.FromBinary(receivedAt);
            var handledAtDate = DateTime.FromBinary(handledAt);
            _repository.SaveTime((receivedAtDate-sentAtDate).TotalMilliseconds, (handledAtDate-prepareToSendDate).TotalMilliseconds);
            await Task.CompletedTask;
        }
    }
}
