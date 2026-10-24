using ParqueoMvc.Models.PRQ;
using ParqueoMvc.Repositories.Interfaces;
using ParqueoMvc.Repositories.Models;

namespace ParqueoMvc.Repositories.Json;

public sealed class IngresoAutomovilJsonRepository(
    JsonDataSource dataSource,
    IParqueoRepository parqueoRepository) : IIngresoAutomovilRepository
{
    public async Task<PrqIngresoAutomovil> InsertarAsync(
        PrqIngresoAutomovil ingreso,
        CancellationToken cancellationToken = default)
    {
        EntidadValidator.Validar(ingreso);
        EntidadValidator.ValidarClaveNueva(ingreso.Consecutivo, nameof(ingreso.Consecutivo));

        return await dataSource.ModificarIngresosAsync(
            async elementos =>
            {
                await ValidarRelacionesAsync(ingreso, cancellationToken);
                var nuevo = new PrqIngresoAutomovil
                {
                    Consecutivo = elementos.Count == 0
                        ? 1
                        : checked(elementos.Max(item => item.Consecutivo) + 1),
                    IdParqueo = ingreso.IdParqueo,
                    IdAutomovil = ingreso.IdAutomovil,
                    FechaHoraEntrada = ingreso.FechaHoraEntrada,
                    FechaHoraSalida = ingreso.FechaHoraSalida
                };
                elementos.Add(nuevo);
                return nuevo;
            },
            cancellationToken);
    }

    public async Task ActualizarAsync(
        PrqIngresoAutomovil ingreso,
        CancellationToken cancellationToken = default)
    {
        EntidadValidator.Validar(ingreso);

        await dataSource.ModificarIngresosAsync(
            async elementos =>
            {
                await ValidarRelacionesAsync(ingreso, cancellationToken);
                var existente = elementos.SingleOrDefault(
                    item => item.Consecutivo == ingreso.Consecutivo)
                    ?? throw new KeyNotFoundException(
                        $"No existe el ingreso de automóvil con consecutivo {ingreso.Consecutivo}.");

                existente.IdParqueo = ingreso.IdParqueo;
                existente.IdAutomovil = ingreso.IdAutomovil;
                existente.FechaHoraEntrada = ingreso.FechaHoraEntrada;
                existente.FechaHoraSalida = ingreso.FechaHoraSalida;
                return true;
            },
            cancellationToken);
    }

    public async Task EliminarAsync(
        int consecutivo,
        CancellationToken cancellationToken = default)
    {
        await dataSource.ModificarIngresosAsync(
            elementos =>
            {
                var existente = elementos.SingleOrDefault(
                    item => item.Consecutivo == consecutivo)
                    ?? throw new KeyNotFoundException(
                        $"No existe el ingreso de automóvil con consecutivo {consecutivo}.");
                elementos.Remove(existente);
                return Task.FromResult(true);
            },
            cancellationToken);
    }

    public async Task<PrqIngresoAutomovil?> ObtenerPorIdAsync(
        int consecutivo,
        CancellationToken cancellationToken = default)
    {
        var ingresos = await ObtenerIngresosRelacionadosAsync(cancellationToken);
        return ingresos.SingleOrDefault(ingreso => ingreso.Consecutivo == consecutivo);
    }

    public async Task<IReadOnlyList<IngresoAutomovilConsulta>> ObtenerIngresosPorTipoYFechasAsync(
        string tipo,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        ConsultaValidator.ValidarRango(desde, hasta);
        ArgumentException.ThrowIfNullOrWhiteSpace(tipo);
        var ingresos = await ObtenerIngresosRelacionadosAsync(cancellationToken);

        return await CrearResultadosAsync(
            ingresos.Where(ingreso =>
                ingreso.FechaHoraEntrada >= desde
                && ingreso.FechaHoraEntrada <= hasta
                && TextoAproximado.Coincide(ingreso.Automovil.Tipo, tipo)).ToList(),
            cancellationToken);
    }

    public async Task<IReadOnlyList<IngresoAutomovilConsulta>> ObtenerIngresosPorProvinciaYFechasAsync(
        string nombreProvincia,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        ConsultaValidator.ValidarRango(desde, hasta);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombreProvincia);
        var ingresos = await ObtenerIngresosRelacionadosAsync(cancellationToken);

        return await CrearResultadosAsync(
            ingresos.Where(ingreso =>
                ingreso.FechaHoraEntrada >= desde
                && ingreso.FechaHoraEntrada <= hasta
                && TextoAproximado.Coincide(ingreso.Parqueo.NombreProvincia, nombreProvincia))
                .ToList(),
            cancellationToken);
    }

    private async Task<List<PrqIngresoAutomovil>> ObtenerIngresosRelacionadosAsync(
        CancellationToken cancellationToken)
    {
        var ingresos = await dataSource.ObtenerIngresosAsync(cancellationToken);
        var automoviles = (await dataSource.ObtenerAutomovilesAsync(cancellationToken))
            .ToDictionary(automovil => automovil.Id);
        var parqueos = (await dataSource.ObtenerParqueosAsync(cancellationToken))
            .ToDictionary(parqueo => parqueo.Id);

        foreach (var ingreso in ingresos)
        {
            if (!automoviles.TryGetValue(ingreso.IdAutomovil, out var automovil))
            {
                throw new InvalidDataException(
                    $"El ingreso {ingreso.Consecutivo} referencia el automóvil inexistente {ingreso.IdAutomovil}.");
            }

            if (!parqueos.TryGetValue(ingreso.IdParqueo, out var parqueo))
            {
                throw new InvalidDataException(
                    $"El ingreso {ingreso.Consecutivo} referencia el parqueo inexistente {ingreso.IdParqueo}.");
            }

            ingreso.Automovil = automovil;
            ingreso.Parqueo = parqueo;
        }

        return ingresos.ToList();
    }

    private async Task ValidarRelacionesAsync(
        PrqIngresoAutomovil ingreso,
        CancellationToken cancellationToken)
    {
        var automoviles = await dataSource.ObtenerAutomovilesAsync(cancellationToken);
        if (!automoviles.Any(automovil => automovil.Id == ingreso.IdAutomovil))
        {
            throw new InvalidDataException(
                $"No existe el automóvil con id {ingreso.IdAutomovil}.");
        }

        if (await parqueoRepository.ObtenerPorIdAsync(
                ingreso.IdParqueo,
                cancellationToken) is null)
        {
            throw new InvalidDataException(
                $"No existe el parqueo con id {ingreso.IdParqueo}.");
        }
    }

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
