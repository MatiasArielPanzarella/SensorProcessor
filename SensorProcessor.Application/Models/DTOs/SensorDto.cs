namespace SensorProcessor.Application.Models.DTOs
{
    public record SensorDto(
     int Index,
     Guid Id,
     bool IsActive,
     string Zone,
     decimal Value
 );
}
