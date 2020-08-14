namespace NanoMessageBus.Compressor.MessagePack.Test
{
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class MessagePackCompressorExtensionsTest
    {
        [Fact]
        public void AddNanoMessageBusDeflateJsonCompressor()
        {
            // arrange
            var serviceCollection = new ServiceCollection();
            
            // act
            serviceCollection.AddNanoMessageBusMessagePackCompressor();
            var container = serviceCollection.BuildServiceProvider();

            // assert
            Assert.IsType<MessagePackCompressor>(container.GetService<ICompressor>());
            Assert.NotNull(container.GetService<ICompressor>());
            Assert.Equal(container.GetService<ICompressor>(), container.GetService<ICompressor>());
        } 
    }
}
