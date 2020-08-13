namespace NanoMessageBus.BenchmarkService.Interfaces
{
    using System;
    using System.Threading.Tasks;

    public interface IBenchmarkRepository
    {
        void SaveInfo(Guid messageId, int messageSize, long prepareToSendAt, long sentAt, long receivedAt, long handledAt);

        Task ExportFilteredDataAsync(int totalMessages, string compressEngine);
    }
}
