using SensorProcessor.Application.Models.DTOs;

namespace SensorProcessor.Application.Contracts
{
    public interface ISensorWriter
    {
        Task WriteAsync(IAsyncEnumerable<SensorDto> sensors, string outputPath, CancellationToken ct = default);
    }
}
