namespace NanoMessageBus.Compressor.DeflateJson.Test
{
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class DeflateJsonCompressorExtensionsTest
    {
        [Fact]
        public void AddNanoMessageBusDeflateJsonCompressor()
        {
            // arrange
            var serviceCollection = new ServiceCollection();
            
            // act
            serviceCollection.AddNanoMessageBusDeflateJsonCompressor();
            var container = serviceCollection.BuildServiceProvider();

            // assert
            Assert.IsType<DeflateJsonCompressor>(container.GetService<ICompressor>());
            Assert.NotNull(container.GetService<ICompressor>());
            Assert.Equal(container.GetService<ICompressor>(), container.GetService<ICompressor>());
        } 
    }
}
