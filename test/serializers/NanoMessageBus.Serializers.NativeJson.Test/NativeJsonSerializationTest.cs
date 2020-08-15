namespace NanoMessageBus.Serializers.NativeJson.Test
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Abstractions.Enums;
    using Abstractions.Interfaces;
    using NativeJson;
    using Xunit;

    public class SubMessage
    {
        public int Property1 { get; set; }

        public double Property2 { get; set; }

        public string Property3 { get; set; }
    }

    public class Message : IMessage
    {
        public int Property1 { get; set; }

        public double Property2 { get; set; }

        public string Property3 { get; set; }

        public List<int> Property4 { get; set; }

        public List<SubMessage> Property5 { get; set; }
    }

    public class NativeJsonSerializationTest
    {
        [Fact]
        public void Identification()
        {
            // act
            var compressor = new NativeJsonSerialization();

            // assert
            Assert.Equal(SerializationEngine.NativeJson, compressor.Identification);
        }

        [Fact]
        public async Task CompressMessageAsync()
        {
            // arrange
            var message = CreateMessage();
            var compressor = new NativeJsonSerialization();
            var expectedCompressedMessage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            // act
            var result = await compressor.SerializeMessageAsync(message);

            // assert
            Assert.Equal(expectedCompressedMessage, result);
        }

        [Fact]
        public async Task DecompressMessageAsync()
        {
            // arrange
            var message = CreateMessage();
            var compressor = new NativeJsonSerialization();
            var compressedMessage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            // act
            var result = await compressor.DeserializeMessageAsync(compressedMessage, typeof(Message));

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
