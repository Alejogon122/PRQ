using Microsoft.EntityFrameworkCore;
using ParqueoMvc.Data;
using ParqueoMvc.Models.PRQ;
using ParqueoMvc.Repositories.Interfaces;
using ParqueoMvc.Repositories.Models;

namespace ParqueoMvc.Repositories.Database;

public sealed class ParqueoRepository(ParqueoDbContext dbContext) : IParqueoRepository
{
    public Task<PrqParqueo?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        dbContext.Parqueos
            .AsNoTracking()
            .SingleOrDefaultAsync(parqueo => parqueo.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PrqParqueo>> BuscarAsync(
        string? nombreProvincia = null,
        string? nombreParqueo = null,
        decimal? precioDesde = null,
        decimal? precioHasta = null,
        CancellationToken cancellationToken = default)
    {
        ConsultaValidator.ValidarRango(precioDesde, precioHasta, "precio");

        var consulta = dbContext.Parqueos.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(nombreProvincia))
        {
            consulta = consulta.Where(parqueo =>
                EF.Functions.ILike(parqueo.NombreProvincia, $"%{nombreProvincia.Trim()}%"));
        }

        if (!string.IsNullOrWhiteSpace(nombreParqueo))
        {
            consulta = consulta.Where(parqueo =>
                EF.Functions.ILike(parqueo.Nombre, $"%{nombreParqueo.Trim()}%"));
        }

        if (precioDesde.HasValue)
        {
            consulta = consulta.Where(parqueo => parqueo.PrecioPorHora >= precioDesde.Value);
        }

        if (precioHasta.HasValue)
        {
            consulta = consulta.Where(parqueo => parqueo.PrecioPorHora <= precioHasta.Value);
        }

        return await consulta
            .OrderBy(parqueo => parqueo.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<decimal?> ObtenerPrecioPorHoraPorParqueo(
        int idParqueo,
        CancellationToken cancellationToken = default) =>
        dbContext.Parqueos
            .AsNoTracking()
            .Where(parqueo => parqueo.Id == idParqueo)
            .Select(parqueo => (decimal?)parqueo.PrecioPorHora)
            .SingleOrDefaultAsync(cancellationToken);
}
