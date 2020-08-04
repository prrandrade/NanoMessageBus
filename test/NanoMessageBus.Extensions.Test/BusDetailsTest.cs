namespace NanoMessageBus.Extensions.Test
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestPlatform.TestExecutor;
    using Xunit;

    public class BusDetailsTest
    {
        [Theory]
        [InlineData("ident1", 0)]
        [InlineData("ident2", 1)]
        public void GetExchangeName(string identification, uint i)
        {
            // act
            var result = BusDetails.GetExchangeName(identification, i);

            // assert
            Assert.Equal($"exchange.{identification}.{i}", result);
        }

        [Theory]
        [InlineData("ident1", 0)]
        [InlineData("ident2", 1)]
        public void GetQueueName(string identification, uint i)
        {
            // act
            var result = BusDetails.GetQueueName(identification, i);

            // assert
            Assert.Equal($"queue.{identification}.{i}", result);
        }

        [Theory]
        [InlineData("0,1,2,3", 10, new uint[] { 0, 1, 2, 3 })]
        [InlineData("0-5,6,8", 10, new uint[] { 0, 1, 2, 3, 4, 5, 6, 8 })]
        [InlineData("0-3,6-9", 10, new uint[] { 0, 1, 2, 3, 6, 7, 8, 9 })]
        [InlineData("1-3,6-6", 10, new uint[] { 1, 2, 3, 6 })]
        public void GetListenedShardsFromPropertyValue(string shardCommand, uint maxShard, uint[] expectedResult)
        {
            // act
            var result = BusDetails.GetListenedShardsFromPropertyValue(shardCommand, maxShard);

            // assert
            Assert.Equal(expectedResult.ToList(), result);
        }

        [Theory]
        [InlineData("1,2,3,11", 10, "Invalid shard 11. It must be less than maxShard 10!")]
        [InlineData("7,10", 10, "Invalid shard 10. It must be less than maxShard 10!")]
        [InlineData("5-1", 10, "Invalid interval 5-1!")]
        [InlineData("10-4", 10, "Invalid interval 10-4!")]
        [InlineData("7-11", 10, "Invalid interval 7-11. The interval must be less than maxShard 10!")]
        [InlineData("11-12", 10, "Invalid interval 11-12. The interval must be less than maxShard 10!")]
        public void GetListenedShardsFromPropertyValue_Errors(string shardCommand, uint maxShard, string expectedErrorMessage)
        {
            // act
            var result = Record.Exception(() => BusDetails.GetListenedShardsFromPropertyValue(shardCommand, maxShard));

            // assert
            Assert.IsType<ArgumentException>(result);
            Assert.Equal(expectedErrorMessage, result.Message);
        }

        [Theory]
        [InlineData("service", new[] { "service" })]
        [InlineData("service1,service2", new[] { "service1", "service2" })]
        [InlineData("service1,,service2", new[] { "service1", "service2" })]
        [InlineData("service1,   ,service2", new[] { "service1", "service2" })]
        public void GetListenedServicesFromPropertyValue(string serviceCommand, string[] expectedResult)
        {
            // act
            var result = BusDetails.GetListenedServicesFromPropertyValue(serviceCommand);

            // assert
            Assert.Equal(result, expectedResult.ToList());
        }
    }
}
