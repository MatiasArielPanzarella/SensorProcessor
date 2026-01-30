using System.Text.Json;
using SensorProcessor.Application.Contracts;
using SensorProcessor.Application.UseCases;
using SensorProcessor.Infrastructure.Readers;
using SensorProcessor.Infrastructure.Writers;

Console.WriteLine("=== Sensor Processor ===");
Console.WriteLine("Ingrese la ruta del archivo sensors.json o la carpeta que lo contiene:");
var input = Console.ReadLine()?.Trim().Trim('"');//Leo una línea de consola de forma segura, eliminando espacios y comillas externas para normalizar el input.

if (string.IsNullOrWhiteSpace(input))
{
    Console.WriteLine("Ruta inválida.");
    return;
}

string basePath;
string inputPath;

/* el siguiente bloque de codigo interpreta el input permitiendo pasar tanto un archivo como una 
 * carpeta. Valida la existencia, arma las rutas correspondientes y corta la ejecución de 
 * forma temprana con mensajes claros si el input es inválido
 */
if (File.Exists(input))
{
    inputPath = input;
    basePath = Path.GetDirectoryName(input)!;//Uso el null-forgiving operator para indicar al compilador que, dado el contexto de validación previa, el valor no será null
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

Console.WriteLine("Ingrese los formatos de salida (csv, xml o csv,xml):");
var formatInput = Console.ReadLine()?.ToLowerInvariant();//Convierte un string a minúsculas para evitar problemas de mayúsculas/minúsculas al comparar

if (string.IsNullOrWhiteSpace(formatInput))
{
    Console.WriteLine("Debe especificar al menos un formato.");
    return;
}

var formats = formatInput
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Distinct();//spliteo el string separado por comas en un array removiendo entradas vacías y espacios en blanco

var outputs = new Dictionary<ISensorWriter, string>();//Diccionario para mapear cada escritor con su path de salida.
                                                      //Referencia a un objeto concreto que implementa ISensorWriter

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

            outputs.Add(new CsvSensorWriter(), csvPath);//Agrega el escritor CSV y su path al diccionario
            break;

        case "xml":
            Console.WriteLine("Ingrese el path de salida para XML (archivo o carpeta, Enter = misma carpeta):");
            var xmlPath = Console.ReadLine()?.Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(xmlPath))
                xmlPath = Path.Combine(basePath, "sensors.xml");
            else if (Directory.Exists(xmlPath))
                xmlPath = Path.Combine(xmlPath, "sensors.xml");

            outputs.Add(new XmlSensorWriter(), xmlPath);//Agrega el escritor XML y su path al diccionario
            break;

        default:
            Console.WriteLine($"Formato no soportado: {format}");
            return;
    }
}

var reader = new JsonSensorReader();//Crea el lector JSON
var useCase = new ProcessSensorsUseCase(reader, outputs);//Crea el caso de uso con el lector y los escritores configurados por parte del usuario
var stats = await useCase.ExecuteAsync(inputPath);//Ejecuta el procesamiento y obtiene las estadísticas
var statsPath = Path.Combine(basePath, "statistics.json");

var statsJson = JsonSerializer.Serialize(
    stats,
    new JsonSerializerOptions { WriteIndented = true });//Serializa las estadísticas a JSON con indentación (uso de espacios o tabs para mostrar la estructura del código o de un archivo) para mejor legibilidad

await File.WriteAllTextAsync(statsPath, statsJson);
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
