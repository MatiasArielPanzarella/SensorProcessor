using SensorProcessor.Application.Models.DTOs;

namespace SensorProcessor.Application.Contracts
{
    public interface ISensorReader
    {
        IAsyncEnumerable<SensorDto> ReadAsync(string path, CancellationToken ct = default);
    }
}
