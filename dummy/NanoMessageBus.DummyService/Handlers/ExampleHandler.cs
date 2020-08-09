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

        public override async Task RegisterStatisticsAsync(DateTime prepareToSendAt, DateTime sentAt, DateTime receivedAt, DateTime handledAt)
        {

            _repository.SaveTime((receivedAt-sentAt).TotalMilliseconds, (handledAt-prepareToSendAt).TotalMilliseconds);
            await Task.CompletedTask;
        }
    }
}
