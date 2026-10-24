using ParqueoMvc.Models.PRQ;
using ParqueoMvc.Repositories.Interfaces;
using ParqueoMvc.Repositories.Models;

namespace ParqueoMvc.Repositories.Json;

public sealed class ParqueoJsonRepository(JsonDataSource dataSource) : IParqueoRepository
{
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
