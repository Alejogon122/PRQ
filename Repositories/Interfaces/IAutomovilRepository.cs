using ParqueoMvc.Models.PRQ;

namespace ParqueoMvc.Repositories.Interfaces;

public interface IAutomovilRepository
{
    Task<PrqAutomovil?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PrqAutomovil> InsertarAsync(
        PrqAutomovil automovil,
        CancellationToken cancellationToken = default);

    Task ActualizarAsync(
        PrqAutomovil automovil,
        CancellationToken cancellationToken = default);

    Task EliminarAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrqAutomovil>> BuscarAsync(
        string? color = null,
        int? anioDesde = null,
        int? anioHasta = null,
        string? fabricante = null,
        string? tipo = null,
        CancellationToken cancellationToken = default);
}
