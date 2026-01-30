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
            FileMode.Create,//valida que exista....si no existe lo crea y si existe lo sobreescribe
            FileAccess.Write,
            FileShare.None,//ningun otro proceso puede acceder al archivo mientras se escribe
            bufferSize: 4096,//4kb para tamaño buffer
            useAsync: true);//Evita bloquear threads mientras se escribe al disco

        await using var writer = new StreamWriter(stream, Encoding.UTF8);//especifica que se use UTF8 para escribir el archivo asegurando compatibilidad con caracteres especiales

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
