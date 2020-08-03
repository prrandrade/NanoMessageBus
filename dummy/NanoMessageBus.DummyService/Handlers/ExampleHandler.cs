namespace NanoMessageBus.DummyService.Handlers
{
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

        public override async Task Handle(ExampleMessage message)
        {
            _logger.LogInformation(message.MessageContent);
            await Task.CompletedTask;
        }
    }
}
