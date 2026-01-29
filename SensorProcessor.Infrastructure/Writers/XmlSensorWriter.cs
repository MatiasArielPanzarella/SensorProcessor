using System.Text;
using System.Xml;
using SensorProcessor.Application.Contracts;
using SensorProcessor.Application.Models.DTOs;

namespace SensorProcessor.Infrastructure.Writers;

public sealed class XmlSensorWriter : ISensorWriter
{
    public async Task WriteAsync(
        IAsyncEnumerable<SensorDto> sensors,
        string outputPath,
        CancellationToken ct = default)//Uso default para no obligar al consumidor del método a manejar cancelación si no la necesita
    {
        //liberacion de recursos asincrona para FileStream y XmlWriter
        await using var stream = new FileStream(
            outputPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,//evita corrupciones en el proceso de escritura. No hay concurrencia aca.
            bufferSize: 4096,
            useAsync: true); //habilita operaciones asincronas en el stream Le dice a .NET: “Este stream se va a usar con async/ await”. Optimiza I/ O no bloqueante

        using var writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Async = true, //habilita usar WriteStartDocumentAsync, WriteStartElementAsync, etc sino se produce excepcion en runtime
            Indent = true,
            Encoding = Encoding.UTF8
        });

        await writer.WriteStartDocumentAsync(); //<?xml version="1.0" encoding="utf-8"?> header del archivo

        await writer.WriteStartElementAsync(null, "Sensors", null);//<Sensors> comienza nodo raíz

        await foreach (var s in sensors.WithCancellation(ct)) // usado cuando IAsyncEnumerable<T>. No estan todos los elementos de la lista en memoria sino que se van cargando uno a uno ("Dame el próximo sensor… espero… listo, escribo… dame el siguiente…")
        { //el token puede cancelar la iteracion y no bloquea como cuando se leen listas con un foreach
            await writer.WriteStartElementAsync(null, "Sensor", null);

            await writer.WriteElementStringAsync(null, "Index", null, s.Index.ToString());
            await writer.WriteElementStringAsync(null, "Id", null, s.Id.ToString());
            await writer.WriteElementStringAsync(null, "IsActive", null, s.IsActive.ToString());
            await writer.WriteElementStringAsync(null, "Zone", null, s.Zone);
            await writer.WriteElementStringAsync(
                null,
                "Value",
                null,
                s.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));

            await writer.WriteEndElementAsync(); 
        }

        await writer.WriteEndElementAsync(); // cierra el último elemento abierto, </Sensors> finaliza nodo raiz
        await writer.WriteEndDocumentAsync(); // finaliza el documento completo cerrando todas las etiquetas pendientes
    }
}
