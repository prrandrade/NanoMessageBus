namespace NanoMessageBus.Compressor.DeflateJson.Test
{
    using System.IO;
    using System.IO.Compression;
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

    public class DeflateJsonCompressorTest
    {
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
            var compressor = new DeflateJsonCompressor();

            var compressedMessage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var output = new MemoryStream();
            await using var dstream = new DeflateStream(output, CompressionLevel.Optimal);
            dstream.Write(compressedMessage, 0, compressedMessage.Length);
            dstream.Close();
            var expectedResult = output.ToArray();

            // act
            var result = await compressor.CompressMessageAsync(message);

            // assert
            Assert.Equal(expectedResult, result);
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
            var compressor = new DeflateJsonCompressor();

            var output = new MemoryStream();
            await using var dstream = new DeflateStream(output, CompressionLevel.Optimal);
            dstream.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)), 0, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)).Length);
            dstream.Close();
            var compressedMessage = output.ToArray();

            // act
            var result = await compressor.DecompressMessageAsync(compressedMessage, typeof(Message));

            // assert
            Assert.Equal(message.Property1, ((Message)result).Property1);
            Assert.Equal(message.Property2, ((Message)result).Property2);
            Assert.Equal(message.Property3, ((Message)result).Property3);
        }
    }
}
