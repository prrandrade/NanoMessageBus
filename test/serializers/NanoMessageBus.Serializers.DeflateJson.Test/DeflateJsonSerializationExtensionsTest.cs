namespace NanoMessageBus.Serializers.DeflateJson.Test
{
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class DeflateJsonSerializationExtensionsTest
    {
        [Fact]
        public void AddNanoMessageBusDeflateJsonSerialization()
        {
            // arrange
            var serviceCollection = new ServiceCollection();
            
            // act
            serviceCollection.AddNanoMessageBusDeflateJsonSerialization();
            var container = serviceCollection.BuildServiceProvider();

            // assert
            Assert.IsType<DeflateJsonSerialization>(container.GetService<ISerialization>());
            Assert.NotNull(container.GetService<ISerialization>());
            Assert.Equal(container.GetService<ISerialization>(), container.GetService<ISerialization>());
        } 
    }
}
