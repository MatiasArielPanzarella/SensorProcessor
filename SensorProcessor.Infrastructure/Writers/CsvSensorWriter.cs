using System.Text;
using SensorProcessor.Application.Contracts;
using SensorProcessor.Application.Models.DTOs;

namespace SensorProcessor.Infrastructure.Writers;

public sealed class CsvSensorWriter : ISensorWriter
{
    public async Task WriteAsync(
        IAsyncEnumerable<SensorDto> sensors,
        string outputPath,
        CancellationToken ct = default)
    {
        await using var stream = new FileStream(
            outputPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            useAsync: true);

        await using var writer = new StreamWriter(stream, Encoding.UTF8);

        // header
        await writer.WriteLineAsync("Index,Id,IsActive,Zone,Value");

        await foreach (var s in sensors.WithCancellation(ct))
        {
            var line =
                $"{s.Index},{s.Id},{s.IsActive},{s.Zone},{s.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

            await writer.WriteLineAsync(line);
        }
    }
}
