namespace SensorProcessor.Domain.Entities
{
    public record Sensor(
    int Index,
    Guid Id,
    bool IsActive,
    string Zone,
    decimal Value
);
}
