using System.ComponentModel.DataAnnotations.Schema;

namespace ParqueoMvc.Models.PRQ;

public partial class PrqIngresoAutomovil
{
    private TimeSpan? DuracionActual =>
        FechaHoraSalida is null
            ? DateTime.Now - FechaHoraEntrada
            : null;

    [NotMapped]
    public double? DuracionEstadiaMinutos => DuracionActual?.TotalMinutes;

    [NotMapped]
    public double? DuracionEstadiaHoras => DuracionActual?.TotalHours;

    [NotMapped]
    public decimal? MontoTotalAPagar
    {
        get
        {
            var duracion = DuracionActual;
            if (duracion is null || Parqueo is null)
            {
                return null;
            }

            return decimal.Round(
                (decimal)duracion.Value.TotalHours * Parqueo.PrecioPorHora,
                2,
                MidpointRounding.AwayFromZero);
        }
    }
}
