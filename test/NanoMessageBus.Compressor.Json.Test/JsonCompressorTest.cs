namespace NanoMessageBus.Compressor.Json.Test
{
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Abstractions.Interfaces;
    using Xunit;

    public class Message : IMessage
    {
        public int Property1 { get; set; }

        public double Property2 { get; set; }

        public string Property3 { get; set; }
    }

    public class JsonCompressorTest
    {
        [Fact]
        public void Identification()
        {
            // act
            var compressor = new JsonCompressor();

            // assert
            Assert.Equal("Json", compressor.Identification);  
        }

        [Fact]
        public async Task CompressMessageAsync()
        {
            // arrange
            var message = new Message
            {
                Property1 = 1,
                Property2 = 2.5,
                Property3 = "test"
            };
            var compressor = new JsonCompressor();
            var expectedCompressedMessage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            // act
            var result = await compressor.CompressMessageAsync(message);

            // assert
            Assert.Equal(expectedCompressedMessage, result);
        }

        [Fact]
        public async Task DecompressMessageAsync()
        {
            // arrange
            var message = new Message
            {
                Property1 = 1,
                Property2 = 2.5,
                Property3 = "test"
            };
            var compressor = new JsonCompressor();
            var compressedMessage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            // act
            var result = await compressor.DecompressMessageAsync(compressedMessage, typeof(Message));

            // assert
            Assert.Equal(message.Property1, ((Message)result).Property1);
            Assert.Equal(message.Property2, ((Message)result).Property2);
            Assert.Equal(message.Property3, ((Message)result).Property3);
        }
    }
}
