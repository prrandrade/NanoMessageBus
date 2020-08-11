namespace NanoMessageBus.Compressor.Protobuf.Test
{
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class ProtobufCompressorExtensionsTest
    {
        [Fact]
        public void AddNanoMessageBusDeflateJsonCompressor()
        {
            // arrange
            var serviceCollection = new ServiceCollection();
            
            // act
            serviceCollection.AddNanoMessageBusProtobufCompressor();
            var container = serviceCollection.BuildServiceProvider();

            // assert
            Assert.IsType<ProtobufCompressor>(container.GetService<ICompressor>());
            Assert.NotNull(container.GetService<ICompressor>());
            Assert.Equal(container.GetService<ICompressor>(), container.GetService<ICompressor>());
        } 
    }
}
