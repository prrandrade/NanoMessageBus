namespace NanoMessageBus.Compressor.Protobuf.Test
{
    using System.IO;
    using System.Threading.Tasks;
    using Abstractions.Interfaces;
    using ProtoBuf;
    using Xunit;

    [ProtoContract]
    public class Message : IMessage
    {
        [ProtoMember(1)]
        public int Property1 { get; set; }

        [ProtoMember(2)]
        public double Property2 { get; set; }

        [ProtoMember(3)]
        public string Property3 { get; set; }
    }

    public class ProtobufCompressorTest
    {
        [Fact]
        public void Identification()
        {
            // act
            var compressor = new ProtobufCompressor();

            // assert
            Assert.Equal("Protobuf", compressor.Identification);  
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
            var compressor = new ProtobufCompressor();
            var stream = new MemoryStream();
            Serializer.Serialize(stream, message);

            // act
            var result = await compressor.CompressMessageAsync(message);

            // assert
            Assert.Equal(stream.ToArray(), result);
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
            var compressor = new ProtobufCompressor();
            var stream = new MemoryStream();
            Serializer.Serialize(stream, message);

            // act
            var result = await compressor.DecompressMessageAsync(stream.ToArray(), typeof(Message));

            // assert
            Assert.Equal(message.Property1, ((Message)result).Property1);
            Assert.Equal(message.Property2, ((Message)result).Property2);
            Assert.Equal(message.Property3, ((Message)result).Property3);
        }
    }
}
