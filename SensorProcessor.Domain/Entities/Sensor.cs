namespace SensorProcessor.Domain.Entities
{
    //objeto inmutable. Ideal para modelos de datos por valor. 
    public record Sensor(
    int Index,
    Guid Id,
    bool IsActive,
    string Zone,
    decimal Value
    );
}
