using Microsoft.EntityFrameworkCore;
using ParqueoMvc.Data;
using ParqueoMvc.Models.PRQ;
using ParqueoMvc.Repositories.Interfaces;
using ParqueoMvc.Repositories.Models;

namespace ParqueoMvc.Repositories.Database;

public sealed class AutomovilRepository(ParqueoDbContext dbContext) : IAutomovilRepository
{
    public Task<PrqAutomovil?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        dbContext.Automoviles
            .AsNoTracking()
            .SingleOrDefaultAsync(automovil => automovil.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PrqAutomovil>> BuscarAsync(
        string? color = null,
        int? anioDesde = null,
        int? anioHasta = null,
        string? fabricante = null,
        string? tipo = null,
        CancellationToken cancellationToken = default)
    {
        ConsultaValidator.ValidarRango(anioDesde, anioHasta, "año");

        var consulta = dbContext.Automoviles.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(color))
        {
            consulta = consulta.Where(automovil =>
                EF.Functions.ILike(automovil.Color, $"%{color.Trim()}%"));
        }

        if (anioDesde.HasValue)
        {
            consulta = consulta.Where(automovil => automovil.Anio >= anioDesde.Value);
        }

        if (anioHasta.HasValue)
        {
            consulta = consulta.Where(automovil => automovil.Anio <= anioHasta.Value);
        }

        if (!string.IsNullOrWhiteSpace(fabricante))
        {
            consulta = consulta.Where(automovil =>
                EF.Functions.ILike(automovil.Fabricante, $"%{fabricante.Trim()}%"));
        }

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            consulta = consulta.Where(automovil =>
                EF.Functions.ILike(automovil.Tipo, $"%{tipo.Trim()}%"));
        }

        return await consulta
            .OrderBy(automovil => automovil.Id)
            .ToListAsync(cancellationToken);
    }
}
