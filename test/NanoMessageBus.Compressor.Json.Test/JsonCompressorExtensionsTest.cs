namespace NanoMessageBus.Compressor.Json.Test
{
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class JsonCompressorExtensionsTest
    {
        [Fact]
        public void AddNanoMessageBusDeflateJsonCompressor()
        {
            // arrange
            var serviceCollection = new ServiceCollection();
            
            // act
            serviceCollection.AddNanoMessageBusJsonCompressor();
            var container = serviceCollection.BuildServiceProvider();

            // assert
            Assert.IsType<JsonCompressor>(container.GetService<ICompressor>());
            Assert.NotNull(container.GetService<ICompressor>());
            Assert.Equal(container.GetService<ICompressor>(), container.GetService<ICompressor>());
        } 
    }
}
