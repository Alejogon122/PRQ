using ParqueoMvc.Models.PRQ;
using ParqueoMvc.Repositories.Models;

namespace ParqueoMvc.Repositories.Interfaces;

public interface IIngresoAutomovilRepository
{
    Task<PrqIngresoAutomovil?> ObtenerPorIdAsync(
        int consecutivo,
        CancellationToken cancellationToken = default);

    Task<PrqIngresoAutomovil> InsertarAsync(
        PrqIngresoAutomovil ingreso,
        CancellationToken cancellationToken = default);

    Task ActualizarAsync(
        PrqIngresoAutomovil ingreso,
        CancellationToken cancellationToken = default);

    Task EliminarAsync(int consecutivo, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IngresoAutomovilConsulta>> ObtenerIngresosPorTipoYFechasAsync(
        string tipo,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IngresoAutomovilConsulta>> ObtenerIngresosPorProvinciaYFechasAsync(
        string nombreProvincia,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default);
}
