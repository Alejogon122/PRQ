using ParqueoMvc.Models.PRQ;
using ParqueoMvc.Repositories.Interfaces;
using ParqueoMvc.Repositories.Models;

namespace ParqueoMvc.Repositories.Json;

public sealed class AutomovilJsonRepository(JsonDataSource dataSource) : IAutomovilRepository
{
    public async Task<PrqAutomovil?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        (await dataSource.ObtenerAutomovilesAsync(cancellationToken))
        .SingleOrDefault(automovil => automovil.Id == id);

    public async Task<IReadOnlyList<PrqAutomovil>> BuscarAsync(
        string? color = null,
        int? anioDesde = null,
        int? anioHasta = null,
        string? fabricante = null,
        string? tipo = null,
        CancellationToken cancellationToken = default)
    {
        ConsultaValidator.ValidarRango(anioDesde, anioHasta, "año");
        var automoviles = await dataSource.ObtenerAutomovilesAsync(cancellationToken);

        return automoviles
            .Where(automovil =>
                TextoAproximado.Coincide(automovil.Color, color)
                && (!anioDesde.HasValue || automovil.Anio >= anioDesde.Value)
                && (!anioHasta.HasValue || automovil.Anio <= anioHasta.Value)
                && TextoAproximado.Coincide(automovil.Fabricante, fabricante)
                && TextoAproximado.Coincide(automovil.Tipo, tipo))
            .OrderBy(automovil => automovil.Id)
            .ToList();
    }
}
