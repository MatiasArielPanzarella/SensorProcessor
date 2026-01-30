using System.Threading.Channels;
using SensorProcessor.Application.Contracts;
using SensorProcessor.Application.Models.DTOs;
using SensorProcessor.Domain.Models;

namespace SensorProcessor.Application.UseCases;
/*
- Id del sensor que sensó el mayor valor.
- El valor medio.
- El valor medio por zona.
- Cantidad de de sensores activos por zona.
 */
public sealed class ProcessSensorsUseCase //sealed porque es un caso de uso..no deseo que otro la herede. Es un caso particular de la aplicación.
{
    private readonly ISensorReader _reader;
    private readonly Dictionary<ISensorWriter, string> _outputs;

    public ProcessSensorsUseCase(
        ISensorReader reader,
        Dictionary<ISensorWriter, string> outputs)
    {
        _reader = reader;
        _outputs = outputs;
    }

    public async Task<SensorStatistics> ExecuteAsync(
        string inputPath,
        CancellationToken ct = default)
    {
        //concurrencia moderna. Crea un canal independiente para cada escritor en este caso CSV o XML. Un channel es un buffer asincrónico thread-safe.
        //si escribis directo al writer, se bloquea el procesamiento de lectura y se pierde el paralelismo.
        var channels = _outputs.Keys.ToDictionary(
            w => w,
            _ => Channel.CreateUnbounded<SensorDto>() //crea una cola asincrónica sin límite para comunicar productores y consumidores de manera thread-safe
        );
        //Channels esta a partir de net core 3 y para net standard 2.1 en adelante asegura compatibilidad.
        //siendo mejor que implementaciones como lock, semaforos, etc.

        decimal totalValue = 0; //suma de todos los valores
        int count = 0; //contador de sensores procesados

        Guid maxSensorId = Guid.Empty;
        decimal maxValue = decimal.MinValue;
        //métricas globales de las estadisticas para encontrar el sensor con el valor más alto.
        var valueByZone = new Dictionary<string, (decimal sum, int count)>(); //suma y contador por zona. Uso una tupla para evitar crear una clase solo para sum y count
        var activeByZone = new Dictionary<string, int>(); //contador de sensores activos por zona
        //output deseado a modo de ejemplo
        //"ZonaA" → (sum: 120.5, count: 4)
        //"ZonaB" → (sum: 80.0, count: 2)

        var writerTasks = channels.Select(kv =>
            kv.Key.WriteAsync(//para cada channel/writer crea una tarea asincrónica que consume del canal y escribe en el output
                kv.Value.Reader.ReadAllAsync(ct), //le pasa un IAsyncEnumerable para que el writer consuma
                _outputs[kv.Key],
                ct
            )
        );//Crea una tarea asincrónica por cada channel/output para consumir sensores
          //y escribirlos en paralelo. No ejecuta todavía, prepara las tareas.

        await foreach (var dto in _reader.ReadAsync(inputPath, ct))//lee los sensores uno por uno de manera asincrónica
        {
            totalValue += dto.Value;//acumula el valor total
            count++;

            if (dto.Value > maxValue)
            {
                maxValue = dto.Value;
                maxSensorId = dto.Id;
            }//actualiza el sensor con el valor máximo

            if (!valueByZone.TryGetValue(dto.Zone, out var acc))//si no existe la zona en el diccionario, inicializa la tupla
                acc = (0, 0);

            valueByZone[dto.Zone] = (acc.sum + dto.Value, acc.count + 1);//actualiza la suma y el contador por zona

            if (dto.IsActive)//si el sensor está activo, incrementa el contador de activos por zona
            {
                activeByZone[dto.Zone] =
                    activeByZone.GetValueOrDefault(dto.Zone) + 1;//inicializa en 0 si no existe
            }

            foreach (var ch in channels.Values)
            {
                await ch.Writer.WriteAsync(dto, ct);
            }
        }

        foreach (var ch in channels.Values)
            ch.Writer.Complete();

        await Task.WhenAll(writerTasks);//espera a que todas las tareas de escritura terminen
        return new SensorStatistics
        {
            SensorMaxValueId = maxSensorId,
            AverageValue = count == 0 ? 0 : totalValue / count,
            AverageValueByZone = valueByZone.ToDictionary(
                x => x.Key,
                x => x.Value.sum / x.Value.count),
            ActiveSensorsByZone = activeByZone
        };
    }
}
