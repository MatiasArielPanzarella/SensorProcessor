using System.Threading.Channels;
using SensorProcessor.Application.Contracts;
using SensorProcessor.Application.Models.DTOs;
using SensorProcessor.Domain.Models;

namespace SensorProcessor.Application.UseCases;

public sealed class ProcessSensorsUseCase
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
        var channels = _outputs.Keys.ToDictionary(
            w => w,
            _ => Channel.CreateUnbounded<SensorDto>()
        );

        decimal totalValue = 0;
        int count = 0;

        Guid maxSensorId = Guid.Empty;
        decimal maxValue = decimal.MinValue;

        var valueByZone = new Dictionary<string, (decimal sum, int count)>();
        var activeByZone = new Dictionary<string, int>();

        var writerTasks = channels.Select(kv =>
            kv.Key.WriteAsync(
                kv.Value.Reader.ReadAllAsync(ct),
                _outputs[kv.Key],
                ct
            )
        );

        await foreach (var dto in _reader.ReadAsync(inputPath, ct))
        {
            totalValue += dto.Value;
            count++;

            if (dto.Value > maxValue)
            {
                maxValue = dto.Value;
                maxSensorId = dto.Id;
            }

            if (!valueByZone.TryGetValue(dto.Zone, out var acc))
                acc = (0, 0);

            valueByZone[dto.Zone] = (acc.sum + dto.Value, acc.count + 1);

            if (dto.IsActive)
            {
                activeByZone[dto.Zone] =
                    activeByZone.GetValueOrDefault(dto.Zone) + 1;
            }

            foreach (var ch in channels.Values)
            {
                await ch.Writer.WriteAsync(dto, ct);
            }
        }

        foreach (var ch in channels.Values)
            ch.Writer.Complete();

        await Task.WhenAll(writerTasks);
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
