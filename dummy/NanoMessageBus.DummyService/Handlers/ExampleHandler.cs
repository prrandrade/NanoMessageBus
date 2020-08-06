namespace NanoMessageBus.DummyService.Handlers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Receiver.Services;

    public class ExampleHandler : MessageHandlerBase<ExampleMessage>
    {
        private readonly ILogger<ExampleHandler> _logger;

        public ExampleHandler(ILogger<ExampleHandler> logger)
        {
            _logger = logger;
        }

        public override async Task RegisterStatisticsAsync(long prepareToSendAt, long sentAt, long receivedAt, long handledAt)
        {
            var prepareToSendDate = DateTime.FromBinary(prepareToSendAt);
            var sentAtDate = DateTime.FromBinary(sentAt);
            var receivedAtDate = DateTime.FromBinary(receivedAt);
            var handledAtDate = DateTime.FromBinary(handledAt);
            _logger.LogInformation($"Travel time: {(receivedAtDate-sentAtDate).TotalMilliseconds}");
            _logger.LogInformation($"Total time: {(handledAtDate-prepareToSendDate).TotalMilliseconds}");
            await Task.CompletedTask;
        }
    }
}
