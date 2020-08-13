namespace NanoMessageBus.BenchmarkService.Interfaces
{
    using System.Threading.Tasks;

    public interface IBenchmarkRepository
    {
        void SaveInfo(long prepareToSendAt, long sentAt, long receivedAt, long handledAt);

        Task ExportDataAsync();
    }
}
