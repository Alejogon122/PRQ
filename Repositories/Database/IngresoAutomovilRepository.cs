using Microsoft.EntityFrameworkCore;
using ParqueoMvc.Data;
using ParqueoMvc.Models.PRQ;
using ParqueoMvc.Repositories.Interfaces;
using ParqueoMvc.Repositories.Models;

namespace ParqueoMvc.Repositories.Database;

public sealed class IngresoAutomovilRepository(
    ParqueoDbContext dbContext,
    IParqueoRepository parqueoRepository) : IIngresoAutomovilRepository
{
    public Task<PrqIngresoAutomovil?> ObtenerPorIdAsync(
        int consecutivo,
        CancellationToken cancellationToken = default) =>
        dbContext.IngresosAutomoviles
            .AsNoTracking()
            .Include(ingreso => ingreso.Automovil)
            .Include(ingreso => ingreso.Parqueo)
            .SingleOrDefaultAsync(
                ingreso => ingreso.Consecutivo == consecutivo,
                cancellationToken);

    public async Task<IReadOnlyList<IngresoAutomovilConsulta>> ObtenerIngresosPorTipoYFechasAsync(
        string tipo,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        ConsultaValidator.ValidarRango(desde, hasta);
        ArgumentException.ThrowIfNullOrWhiteSpace(tipo);

        var ingresos = await ConsultaBase()
            .Where(ingreso =>
                ingreso.FechaHoraEntrada >= desde
                && ingreso.FechaHoraEntrada <= hasta
                && EF.Functions.ILike(ingreso.Automovil.Tipo, $"%{tipo.Trim()}%"))
            .ToListAsync(cancellationToken);

        return await CrearResultadosAsync(ingresos, cancellationToken);
    }

    public async Task<IReadOnlyList<IngresoAutomovilConsulta>> ObtenerIngresosPorProvinciaYFechasAsync(
        string nombreProvincia,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        ConsultaValidator.ValidarRango(desde, hasta);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombreProvincia);

        var ingresos = await ConsultaBase()
            .Where(ingreso =>
                ingreso.FechaHoraEntrada >= desde
                && ingreso.FechaHoraEntrada <= hasta
                && EF.Functions.ILike(
                    ingreso.Parqueo.NombreProvincia,
                    $"%{nombreProvincia.Trim()}%"))
            .ToListAsync(cancellationToken);

        return await CrearResultadosAsync(ingresos, cancellationToken);
    }

    private IQueryable<PrqIngresoAutomovil> ConsultaBase() =>
        dbContext.IngresosAutomoviles
            .AsNoTracking()
            .Include(ingreso => ingreso.Automovil)
            .Include(ingreso => ingreso.Parqueo);

    private async Task<IReadOnlyList<IngresoAutomovilConsulta>> CrearResultadosAsync(
        IReadOnlyList<PrqIngresoAutomovil> ingresos,
        CancellationToken cancellationToken)
    {
        var precios = new Dictionary<int, decimal>();
        foreach (var idParqueo in ingresos.Select(ingreso => ingreso.IdParqueo).Distinct())
        {
            var precio = await parqueoRepository.ObtenerPrecioPorHoraPorParqueo(
                idParqueo,
                cancellationToken);
            if (!precio.HasValue)
            {
                throw new InvalidDataException(
                    $"No se encontró el precio por hora del parqueo {idParqueo}.");
            }

            precios.Add(idParqueo, precio.Value);
        }

        var ahora = DateTime.Now;
        return ingresos
            .Select(ingreso => IngresoAutomovilConsulta.Crear(
                ingreso,
                precios[ingreso.IdParqueo],
                ahora))
            .ToList();
    }
}
