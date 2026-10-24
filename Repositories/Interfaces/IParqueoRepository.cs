using ParqueoMvc.Models.PRQ;

namespace ParqueoMvc.Repositories.Interfaces;

public interface IParqueoRepository
{
    Task<PrqParqueo?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

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
