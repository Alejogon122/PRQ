using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Hosting;
using ParqueoMvc.Models.PRQ;

namespace ParqueoMvc.Repositories.Json;

public sealed class JsonDataSource(IWebHostEnvironment environment)
{
    private static readonly JsonSerializerOptions JsonOptions = CrearOpcionesJson();

    private readonly SemaphoreSlim _mutationLock = new(1, 1);

    private static JsonSerializerOptions CrearOpcionesJson()
    {
        var resolver = new DefaultJsonTypeInfoResolver();
        resolver.Modifiers.Add(typeInfo =>
        {
            foreach (var property in typeInfo.Properties)
            {
                if (property.AttributeProvider is not System.Reflection.PropertyInfo member)
                {
                    continue;
                }

                var isNavigation =
                    (member.DeclaringType == typeof(PrqAutomovil)
                        && member.Name == nameof(PrqAutomovil.IngresosAutomoviles))
                    || (member.DeclaringType == typeof(PrqParqueo)
                        && member.Name == nameof(PrqParqueo.IngresosAutomoviles))
                    || (member.DeclaringType == typeof(PrqIngresoAutomovil)
                        && member.Name is nameof(PrqIngresoAutomovil.Automovil)
                            or nameof(PrqIngresoAutomovil.Parqueo));
                var isCalculated =
                    member.DeclaringType == typeof(PrqIngresoAutomovil)
                    && member.Name is nameof(PrqIngresoAutomovil.DuracionEstadiaMinutos)
                        or nameof(PrqIngresoAutomovil.DuracionEstadiaHoras)
                        or nameof(PrqIngresoAutomovil.MontoTotalAPagar);

                if (isNavigation || isCalculated)
                {
                    property.ShouldSerialize = static (_, _) => false;
                }
            }
        });

        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            TypeInfoResolver = resolver,
            WriteIndented = true
        };
    }

    public Task<IReadOnlyList<PrqAutomovil>> ObtenerAutomovilesAsync(
        CancellationToken cancellationToken = default) =>
        LeerAsync<PrqAutomovil>("PRQ_AUTOMOVILES.json", cancellationToken);

    public Task<IReadOnlyList<PrqParqueo>> ObtenerParqueosAsync(
        CancellationToken cancellationToken = default) =>
        LeerAsync<PrqParqueo>("PRQ_PARQUEOS.json", cancellationToken);

    public Task<IReadOnlyList<PrqIngresoAutomovil>> ObtenerIngresosAsync(
        CancellationToken cancellationToken = default) =>
        LeerAsync<PrqIngresoAutomovil>("PRQ_INGRESOS_AUTOMOVILES.json", cancellationToken);

    public Task<TResult> ModificarAutomovilesAsync<TResult>(
        Func<List<PrqAutomovil>, Task<TResult>> modificar,
        CancellationToken cancellationToken = default) =>
        ModificarAsync("PRQ_AUTOMOVILES.json", modificar, cancellationToken);

    public Task<TResult> ModificarParqueosAsync<TResult>(
        Func<List<PrqParqueo>, Task<TResult>> modificar,
        CancellationToken cancellationToken = default) =>
        ModificarAsync("PRQ_PARQUEOS.json", modificar, cancellationToken);

    public Task<TResult> ModificarIngresosAsync<TResult>(
        Func<List<PrqIngresoAutomovil>, Task<TResult>> modificar,
        CancellationToken cancellationToken = default) =>
        ModificarAsync("PRQ_INGRESOS_AUTOMOVILES.json", modificar, cancellationToken);

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

    private async Task<TResult> ModificarAsync<T, TResult>(
        string nombreArchivo,
        Func<List<T>, Task<TResult>> modificar,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(modificar);
        await _mutationLock.WaitAsync(cancellationToken);
        string? rutaTemporal = null;
        try
        {
            var ruta = Path.Combine(
                environment.ContentRootPath,
                "Data",
                "Json",
                nombreArchivo);
            var elementos = (await LeerAsync<T>(nombreArchivo, cancellationToken)).ToList();
            var resultado = await modificar(elementos);

            rutaTemporal = $"{ruta}.{Guid.NewGuid():N}.tmp";
            await using (var archivo = new FileStream(
                rutaTemporal,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                4096,
                FileOptions.Asynchronous))
            {
                await JsonSerializer.SerializeAsync(
                    archivo,
                    elementos,
                    JsonOptions,
                    cancellationToken);
                await archivo.FlushAsync(cancellationToken);
            }

            File.Move(rutaTemporal, ruta, overwrite: true);
            return resultado;
        }
        finally
        {
            if (rutaTemporal is not null && File.Exists(rutaTemporal))
            {
                File.Delete(rutaTemporal);
            }

            _mutationLock.Release();
        }
    }
}
