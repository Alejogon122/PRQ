using System.Text.Json;
using ParqueoMvc.Models.PRQ;

namespace ParqueoMvc.Repositories.Json;

public sealed class JsonDataSource(IWebHostEnvironment environment)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public Task<IReadOnlyList<PrqAutomovil>> ObtenerAutomovilesAsync(
        CancellationToken cancellationToken = default) =>
        LeerAsync<PrqAutomovil>("PRQ_AUTOMOVILES.json", cancellationToken);

    public Task<IReadOnlyList<PrqParqueo>> ObtenerParqueosAsync(
        CancellationToken cancellationToken = default) =>
        LeerAsync<PrqParqueo>("PRQ_PARQUEOS.json", cancellationToken);

    public Task<IReadOnlyList<PrqIngresoAutomovil>> ObtenerIngresosAsync(
        CancellationToken cancellationToken = default) =>
        LeerAsync<PrqIngresoAutomovil>("PRQ_INGRESOS_AUTOMOVILES.json", cancellationToken);

    private async Task<IReadOnlyList<T>> LeerAsync<T>(
        string nombreArchivo,
        CancellationToken cancellationToken)
    {
        var ruta = Path.Combine(
            environment.ContentRootPath,
            "Data",
            "Json",
            nombreArchivo);

        await using var archivo = File.OpenRead(ruta);
        return await JsonSerializer.DeserializeAsync<List<T>>(
                   archivo,
                   JsonOptions,
                   cancellationToken)
               ?? throw new InvalidDataException(
                   $"El archivo JSON '{ruta}' no contiene una lista válida.");
    }
}
