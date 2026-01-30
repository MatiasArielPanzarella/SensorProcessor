📦 Process Sensors Challenge
📖 Descripción
Este proyecto procesa información de sensores a partir de un archivo JSON y genera distintos outputs (por ejemplo CSV, JSON, etc.) utilizando un diseño desacoplado, 
asincrónico y orientado a buenas prácticas de .NET. El foco está en:
procesamiento eficiente de archivos grandes, bajo consumo de memoria, uso de abstracciones (interfaces), código fácil de extender y testear.

🧠 Decisiones de diseño clave
IAsyncEnumerable para procesar datos en streaming, Streams + async/await para IO no bloqueante, Interfaces (ISensorReader, ISensorWriter) para desacoplar lectura y escritura
Fail-fast validation del input, InvariantCulture para consistencia entre entornos, Use Case sealed para proteger la lógica de negocio.

🏗️ Arquitectura (alto nivel)
Program
  └── ProcessSensorsUseCase
        ├── ISensorReader
        │     └── JsonSensorReader
        └── ISensorWriter
              ├── CsvSensorWriter
              └── JsonSensorWriter

📂 Estructura del proyecto
/src
 ├── Program.cs
 ├── UseCases
 │    └── ProcessSensorsUseCase.cs
 ├── Readers
 │    ├── ISensorReader.cs
 │    └── JsonSensorReader.cs
 ├── Writers
 │    ├── ISensorWriter.cs
 │    ├── CsvSensorWriter.cs
 │    └── JsonSensorWriter.cs
 └── Models
      └── Sensor.cs

▶️ Cómo ejecutar el proyecto
dotnet run "C:\ruta\al\archivo\sensors.json"
o bien dotnet run "C:\ruta\a\la\carpeta"

📥 Input esperado
Archivo sensors.json con una estructura similar a:
[
  {
    "id": "sensor-1",
    "isActive": true,
    "zone": "A",
    "value": 12.34
  }
]

📤 Output
Archivo de sensores en XML, CSV y archivo de estadisticas.

⚙️ Tecnologías usadas
.NET 6 / 7 / 8, C#, System.Text.Json, System.Threading.Channels, Async / Await, Streams
