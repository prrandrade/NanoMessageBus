namespace NanoMessageBus.Serializers.Protobuf.Test
{
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Protobuf;
    using Xunit;

    public class ProtobufSerializationExtensionsTest
    {
        [Fact]
        public void AddNanoMessageBusDeflateJsonCompressor()
        {
            // arrange
            var serviceCollection = new ServiceCollection();
            
            // act
            serviceCollection.AddNanoMessageBusProtobufSerialization();
            var container = serviceCollection.BuildServiceProvider();

            // assert
            Assert.IsType<ProtobufSerialization>(container.GetService<ISerialization>());
            Assert.NotNull(container.GetService<ISerialization>());
            Assert.Equal(container.GetService<ISerialization>(), container.GetService<ISerialization>());
        } 
    }
}
