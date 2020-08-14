namespace NanoMessageBus.Serializers.MessagePack.Test
{
    using Abstractions.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class MessagePackSerializationExtensionsTest
    {
        [Fact]
        public void AddNanoMessageBusDeflateJsonCompressor()
        {
            // arrange
            var serviceCollection = new ServiceCollection();
            
            // act
            serviceCollection.AddNanoMessageBusMessagePackSerialization();
            var container = serviceCollection.BuildServiceProvider();

            // assert
            Assert.IsType<MessagePackSerialization>(container.GetService<ISerialization>());
            Assert.NotNull(container.GetService<ISerialization>());
            Assert.Equal(container.GetService<ISerialization>(), container.GetService<ISerialization>());
        } 
    }
}
