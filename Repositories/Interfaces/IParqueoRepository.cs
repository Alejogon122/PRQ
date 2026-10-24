using ParqueoMvc.Models.PRQ;

namespace ParqueoMvc.Repositories.Interfaces;

public interface IParqueoRepository
{
    Task<PrqParqueo?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PrqParqueo> InsertarAsync(
        PrqParqueo parqueo,
        CancellationToken cancellationToken = default);

    Task ActualizarAsync(
        PrqParqueo parqueo,
        CancellationToken cancellationToken = default);

    Task EliminarAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrqParqueo>> BuscarAsync(
        string? nombreProvincia = null,
        string? nombreParqueo = null,
        decimal? precioDesde = null,
        decimal? precioHasta = null,
        CancellationToken cancellationToken = default);

    Task<decimal?> ObtenerPrecioPorHoraPorParqueo(
        int idParqueo,
        CancellationToken cancellationToken = default);
}
