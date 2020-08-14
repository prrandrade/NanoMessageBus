namespace NanoMessageBus.Serializers.NativeJson.Test
{
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class NativeJsonSerializerExtensionsTest
    {
        [Fact]
        public void AddNanoMessageBusDeflateJsonCompressor()
        {
            // arrange
            var serviceCollection = new ServiceCollection();
            
            // act
            serviceCollection.AddNanoMessageBusNativeJsonSerializer();
            var container = serviceCollection.BuildServiceProvider();

            // assert
            Assert.IsType<NativeJsonSerialization>(container.GetService<ISerialization>());
            Assert.NotNull(container.GetService<ISerialization>());
            Assert.Equal(container.GetService<ISerialization>(), container.GetService<ISerialization>());
        } 
    }
}
