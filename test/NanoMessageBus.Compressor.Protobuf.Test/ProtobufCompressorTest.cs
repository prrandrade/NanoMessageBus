namespace NanoMessageBus.Compressor.Protobuf.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Abstractions.Interfaces;
    using ProtoBuf;
    using Xunit;

    [ProtoContract]
    public class SubMessage
    {
        [ProtoMember(1)]
        public int Property1 { get; set; }

        [ProtoMember(2)]
        public double Property2 { get; set; }

        [ProtoMember(3)]
        public string Property3 { get; set; }
    }

    [ProtoContract]
    public class Message : IMessage
    {
        [ProtoMember(1)]
        public int Property1 { get; set; }

        [ProtoMember(2)]
        public double Property2 { get; set; }

        [ProtoMember(3)]
        public string Property3 { get; set; }

        [ProtoMember(4)]
        public List<int> Property4 { get; set; }

        [ProtoMember(5)]
        public List<SubMessage> Property5 { get; set; }
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
            var message = CreateMessage();
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
            var message = CreateMessage();
            var compressor = new ProtobufCompressor();
            var stream = new MemoryStream();
            Serializer.Serialize(stream, message);

            // act
            var result = await compressor.DecompressMessageAsync(stream.ToArray(), typeof(Message));

            // assert
            Assert.True(CompareMessages((Message)result, message));
        }

        private static Message CreateMessage()
        {
            var r = new Random();
            return new Message
            {
                Property1 = r.Next(),
                Property2 = r.NextDouble(),
                Property3 = Guid.NewGuid().ToString(),
                Property4 = new List<int>
                {
                    r.Next(),
                    r.Next(),
                    r.Next()
                },
                Property5 = new List<SubMessage>
                {
                    new SubMessage
                    {
                        Property1 = r.Next(),
                        Property2 = r.NextDouble(),
                        Property3 = Guid.NewGuid().ToString(),
                    },
                    new SubMessage
                    {
                        Property1 = r.Next(),
                        Property2 = r.NextDouble(),
                        Property3 = Guid.NewGuid().ToString(),
                    },
                    new SubMessage
                    {
                        Property1 = r.Next(),
                        Property2 = r.NextDouble(),
                        Property3 = Guid.NewGuid().ToString(),
                    }
                }
            };
        }

        private static bool CompareMessages(Message m1, Message m2)
        {
            if (m1.Property1 != m2.Property1) return false;
            if (m1.Property2 != m2.Property2) return false;
            if (m1.Property3 != m2.Property3) return false;

            if (m1.Property4[0] != m2.Property4[0]) return false;
            if (m1.Property4[1] != m2.Property4[1]) return false;
            if (m1.Property4[2] != m2.Property4[2]) return false;

            if (m1.Property5[0].Property1 != m2.Property5[0].Property1) return false;
            if (m1.Property5[0].Property2 != m2.Property5[0].Property2) return false;
            if (m1.Property5[0].Property3 != m2.Property5[0].Property3) return false;

            if (m1.Property5[1].Property1 != m2.Property5[1].Property1) return false;
            if (m1.Property5[1].Property2 != m2.Property5[1].Property2) return false;
            if (m1.Property5[1].Property3 != m2.Property5[1].Property3) return false;

            if (m1.Property5[2].Property1 != m2.Property5[2].Property1) return false;
            if (m1.Property5[2].Property2 != m2.Property5[2].Property2) return false;
            if (m1.Property5[2].Property3 != m2.Property5[2].Property3) return false;

            return true;
        }
    }
}
