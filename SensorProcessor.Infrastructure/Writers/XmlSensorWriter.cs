using System.Text;
using System.Xml;
using SensorProcessor.Application.Contracts;
using SensorProcessor.Application.Models.DTOs;

namespace SensorProcessor.Infrastructure.Writers;

public sealed class XmlSensorWriter : ISensorWriter
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

        using var writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Async = true,
            Indent = true,
            Encoding = Encoding.UTF8
        });

        await writer.WriteStartDocumentAsync();
        await writer.WriteStartElementAsync(null, "Sensors", null);

        await foreach (var s in sensors.WithCancellation(ct))
        {
            await writer.WriteStartElementAsync(null, "Sensor", null);

            await writer.WriteElementStringAsync(null, "Index", null, s.Index.ToString());
            await writer.WriteElementStringAsync(null, "Id", null, s.Id.ToString());
            await writer.WriteElementStringAsync(null, "IsActive", null, s.IsActive.ToString());
            await writer.WriteElementStringAsync(null, "Zone", null, s.Zone);
            await writer.WriteElementStringAsync(
                null,
                "Value",
                null,
                s.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));

            await writer.WriteEndElementAsync(); // Sensor
        }

        await writer.WriteEndElementAsync(); // Sensors
        await writer.WriteEndDocumentAsync();
    }
}
