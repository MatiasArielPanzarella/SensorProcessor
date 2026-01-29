using SensorProcessor.Application.Models.DTOs;

namespace SensorProcessor.Application.Contracts
{
    //Todos los metodos asincronicos deben contar con Token de cancelacion. Es un mecanismo de .NET para cancelar operaciones en ejecución
    //esta cancelacion puede ser por parte del usuario con un boton, por timeout, por alguna falla del sistema, 
    //o simplemente porque se cerro el browser en una aplicacion web y ya no es necesario continuar con la operacion
    public interface ISensorWriter
    {
        Task WriteAsync(IAsyncEnumerable<SensorDto> sensors, string outputPath, CancellationToken ct = default);
    }
}
