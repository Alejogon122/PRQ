using ParqueoMvc.Models.PRQ;

namespace ParqueoMvc.Repositories.Interfaces;

public interface IAutomovilRepository
{
    Task<PrqAutomovil?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrqAutomovil>> BuscarAsync(
        string? color = null,
        int? anioDesde = null,
        int? anioHasta = null,
        string? fabricante = null,
        string? tipo = null,
        CancellationToken cancellationToken = default);
}
