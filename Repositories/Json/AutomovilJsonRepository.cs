using ParqueoMvc.Models.PRQ;
using ParqueoMvc.Repositories.Interfaces;
using ParqueoMvc.Repositories.Models;

namespace ParqueoMvc.Repositories.Json;

public sealed class AutomovilJsonRepository(JsonDataSource dataSource) : IAutomovilRepository
{
    public async Task<PrqAutomovil> InsertarAsync(
        PrqAutomovil automovil,
        CancellationToken cancellationToken = default)
    {
        EntidadValidator.Validar(automovil);
        EntidadValidator.ValidarClaveNueva(automovil.Id, nameof(automovil.Id));

        return await dataSource.ModificarAutomovilesAsync(
            elementos =>
            {
                var nuevo = new PrqAutomovil
                {
                    Id = elementos.Count == 0 ? 1 : checked(elementos.Max(item => item.Id) + 1),
                    Color = automovil.Color,
                    Anio = automovil.Anio,
                    Fabricante = automovil.Fabricante,
                    Tipo = automovil.Tipo
                };
                elementos.Add(nuevo);
                return Task.FromResult(nuevo);
            },
            cancellationToken);
    }

    public async Task ActualizarAsync(
        PrqAutomovil automovil,
        CancellationToken cancellationToken = default)
    {
        EntidadValidator.Validar(automovil);

        await dataSource.ModificarAutomovilesAsync(
            elementos =>
            {
                var existente = elementos.SingleOrDefault(item => item.Id == automovil.Id)
                    ?? throw new KeyNotFoundException(
                        $"No existe el automóvil con id {automovil.Id}.");

                existente.Color = automovil.Color;
                existente.Anio = automovil.Anio;
                existente.Fabricante = automovil.Fabricante;
                existente.Tipo = automovil.Tipo;
                return Task.FromResult(true);
            },
            cancellationToken);
    }

    public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        await dataSource.ModificarAutomovilesAsync(
            async elementos =>
            {
                var existente = elementos.SingleOrDefault(item => item.Id == id)
                    ?? throw new KeyNotFoundException($"No existe el automóvil con id {id}.");
                var ingresos = await dataSource.ObtenerIngresosAsync(cancellationToken);
                if (ingresos.Any(ingreso => ingreso.IdAutomovil == id))
                {
                    throw new InvalidOperationException(
                        $"No se puede eliminar el automóvil {id} porque tiene ingresos asociados.");
                }

                elementos.Remove(existente);
                return true;
            },
            cancellationToken);
    }

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
