using ParqueoMvc.Models.PRQ;
using ParqueoMvc.Repositories.Interfaces;
using ParqueoMvc.Repositories.Models;

namespace ParqueoMvc.Repositories.Json;

public sealed class ParqueoJsonRepository(JsonDataSource dataSource) : IParqueoRepository
{
    public async Task<PrqParqueo> InsertarAsync(
        PrqParqueo parqueo,
        CancellationToken cancellationToken = default)
    {
        EntidadValidator.Validar(parqueo);
        EntidadValidator.ValidarClaveNueva(parqueo.Id, nameof(parqueo.Id));

        return await dataSource.ModificarParqueosAsync(
            elementos =>
            {
                var nuevo = new PrqParqueo
                {
                    Id = elementos.Count == 0 ? 1 : checked(elementos.Max(item => item.Id) + 1),
                    NombreProvincia = parqueo.NombreProvincia,
                    Nombre = parqueo.Nombre,
                    PrecioPorHora = parqueo.PrecioPorHora
                };
                elementos.Add(nuevo);
                return Task.FromResult(nuevo);
            },
            cancellationToken);
    }

    public async Task ActualizarAsync(
        PrqParqueo parqueo,
        CancellationToken cancellationToken = default)
    {
        EntidadValidator.Validar(parqueo);

        await dataSource.ModificarParqueosAsync(
            elementos =>
            {
                var existente = elementos.SingleOrDefault(item => item.Id == parqueo.Id)
                    ?? throw new KeyNotFoundException(
                        $"No existe el parqueo con id {parqueo.Id}.");

                existente.NombreProvincia = parqueo.NombreProvincia;
                existente.Nombre = parqueo.Nombre;
                existente.PrecioPorHora = parqueo.PrecioPorHora;
                return Task.FromResult(true);
            },
            cancellationToken);
    }

    public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        await dataSource.ModificarParqueosAsync(
            async elementos =>
            {
                var existente = elementos.SingleOrDefault(item => item.Id == id)
                    ?? throw new KeyNotFoundException($"No existe el parqueo con id {id}.");
                var ingresos = await dataSource.ObtenerIngresosAsync(cancellationToken);
                if (ingresos.Any(ingreso => ingreso.IdParqueo == id))
                {
                    throw new InvalidOperationException(
                        $"No se puede eliminar el parqueo {id} porque tiene ingresos asociados.");
                }

                elementos.Remove(existente);
                return true;
            },
            cancellationToken);
    }

    public async Task<PrqParqueo?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        (await dataSource.ObtenerParqueosAsync(cancellationToken))
        .SingleOrDefault(parqueo => parqueo.Id == id);

    public async Task<IReadOnlyList<PrqParqueo>> BuscarAsync(
        string? nombreProvincia = null,
        string? nombreParqueo = null,
        decimal? precioDesde = null,
        decimal? precioHasta = null,
        CancellationToken cancellationToken = default)
    {
        ConsultaValidator.ValidarRango(precioDesde, precioHasta, "precio");
        var parqueos = await dataSource.ObtenerParqueosAsync(cancellationToken);

        return parqueos
            .Where(parqueo =>
                TextoAproximado.Coincide(parqueo.NombreProvincia, nombreProvincia)
                && TextoAproximado.Coincide(parqueo.Nombre, nombreParqueo)
                && (!precioDesde.HasValue || parqueo.PrecioPorHora >= precioDesde.Value)
                && (!precioHasta.HasValue || parqueo.PrecioPorHora <= precioHasta.Value))
            .OrderBy(parqueo => parqueo.Id)
            .ToList();
    }

    public async Task<decimal?> ObtenerPrecioPorHoraPorParqueo(
        int idParqueo,
        CancellationToken cancellationToken = default) =>
        (await ObtenerPorIdAsync(idParqueo, cancellationToken))?.PrecioPorHora;
}
