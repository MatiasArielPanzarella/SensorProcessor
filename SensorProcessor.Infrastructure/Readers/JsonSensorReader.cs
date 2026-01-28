using System.Runtime.CompilerServices;
using System.Text.Json;
using SensorProcessor.Application.Contracts;
using SensorProcessor.Application.Models.DTOs;

namespace SensorProcessor.Infrastructure.Readers
{
    public sealed class JsonSensorReader : ISensorReader
    {
        private readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async IAsyncEnumerable<SensorDto> ReadAsync(
            string path,
            //opcion de poder abortar el procesamiento completo de manera controlada en operaciones IO largas.
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            await using var stream = File.OpenRead(path);

            await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<JsonElement>(stream, _options, ct))
            {
                if (item.ValueKind != JsonValueKind.Object)
                    continue;
                //evitar, de ser posible, tener todo el archivo en memoria. Vas pasando elemento por elemento sin tener que esperar a toda la lista.
                yield return new SensorDto(
                    Index: item.GetProperty("index").GetInt32(),
                    Id: item.GetProperty("id").GetGuid(),
                    IsActive: item.GetProperty("isActive").GetBoolean(),
                    Zone: item.GetProperty("zone").GetString()!,
                    Value: decimal.Parse(
                        item.GetProperty("value").GetString()!,
                        System.Globalization.CultureInfo.InvariantCulture)
                );
            }
        }
    }
}
