namespace SensorProcessor.Domain.Models
{
    public class SensorStatistics
    {
        public Guid SensorMaxValueId { get; init; }
        public decimal AverageValue { get; init; }
        public Dictionary<string, decimal> AverageValueByZone { get; init; } = new();
        public Dictionary<string, int> ActiveSensorsByZone { get; init; } = new();
    }
}
