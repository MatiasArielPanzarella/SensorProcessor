using System.Text.Json;
using SensorProcessor.Application.Contracts;
using SensorProcessor.Application.UseCases;
using SensorProcessor.Infrastructure.Readers;
using SensorProcessor.Infrastructure.Writers;

Console.WriteLine("=== Sensor Processor ===");

// =====================================================
// INPUT 1: archivo sensors.json O carpeta
// =====================================================
Console.WriteLine("Ingrese la ruta del archivo sensors.json o la carpeta que lo contiene:");
var input = Console.ReadLine()?.Trim().Trim('"');

if (string.IsNullOrWhiteSpace(input))
{
    Console.WriteLine("Ruta inválida.");
    return;
}

string basePath;
string inputPath;

if (File.Exists(input))
{
    inputPath = input;
    basePath = Path.GetDirectoryName(input)!;
}
else if (Directory.Exists(input))
{
    basePath = input;
    inputPath = Path.Combine(basePath, "sensors.json");

    if (!File.Exists(inputPath))
    {
        Console.WriteLine($"No se encontró sensors.json en la carpeta: {basePath}");
        return;
    }
}
else
{
    Console.WriteLine("La ruta ingresada no existe.");
    return;
}

// =====================================================
// INPUT 2: formatos de salida
// =====================================================
Console.WriteLine("Ingrese los formatos de salida (csv, xml o csv,xml):");
var formatInput = Console.ReadLine()?.ToLowerInvariant();

if (string.IsNullOrWhiteSpace(formatInput))
{
    Console.WriteLine("Debe especificar al menos un formato.");
    return;
}

var formats = formatInput
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Distinct();

var outputs = new Dictionary<ISensorWriter, string>();

foreach (var format in formats)
{
    switch (format)
    {
        case "csv":
            Console.WriteLine("Ingrese el path de salida para CSV (archivo o carpeta, Enter = misma carpeta):");
            var csvPath = Console.ReadLine()?.Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(csvPath))
                csvPath = Path.Combine(basePath, "sensors.csv");
            else if (Directory.Exists(csvPath))
                csvPath = Path.Combine(csvPath, "sensors.csv");

            outputs.Add(new CsvSensorWriter(), csvPath);
            break;

        case "xml":
            Console.WriteLine("Ingrese el path de salida para XML (archivo o carpeta, Enter = misma carpeta):");
            var xmlPath = Console.ReadLine()?.Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(xmlPath))
                xmlPath = Path.Combine(basePath, "sensors.xml");
            else if (Directory.Exists(xmlPath))
                xmlPath = Path.Combine(xmlPath, "sensors.xml");

            outputs.Add(new XmlSensorWriter(), xmlPath);
            break;

        default:
            Console.WriteLine($"Formato no soportado: {format}");
            return;
    }
}

// =====================================================
// Infraestructura
// =====================================================
var reader = new JsonSensorReader();
var useCase = new ProcessSensorsUseCase(reader, outputs);

// =====================================================
// Ejecutar
// =====================================================
var stats = await useCase.ExecuteAsync(inputPath);

// =====================================================
// statistics.json
// =====================================================
var statsPath = Path.Combine(basePath, "statistics.json");

var statsJson = JsonSerializer.Serialize(
    stats,
    new JsonSerializerOptions { WriteIndented = true });

await File.WriteAllTextAsync(statsPath, statsJson);

// =====================================================
// Output consola
// =====================================================
Console.WriteLine();
Console.WriteLine("=== Estadísticas ===");
Console.WriteLine($"Sensor con mayor valor: {stats.SensorMaxValueId}");
Console.WriteLine($"Valor medio: {stats.AverageValue}");

Console.WriteLine("Valor medio por zona:");
foreach (var z in stats.AverageValueByZone)
    Console.WriteLine($"{z.Key}: {z.Value}");

Console.WriteLine("Sensores activos por zona:");
foreach (var z in stats.ActiveSensorsByZone)
    Console.WriteLine($"{z.Key}: {z.Value}");

Console.WriteLine();
Console.WriteLine("Proceso finalizado correctamente.");
Console.WriteLine($"Archivos generados en: {basePath}");
