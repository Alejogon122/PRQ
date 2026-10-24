using ParqueoMvc.Models.PRQ;

namespace ParqueoMvc.Repositories.Models;

public sealed record IngresoAutomovilConsulta(
    int Consecutivo,
    int IdAutomovil,
    string Color,
    short Anio,
    string Fabricante,
    string Tipo,
    int IdParqueo,
    string NombreProvincia,
    string NombreParqueo,
    DateTime FechaHoraEntrada,
    DateTime? FechaHoraSalida,
    decimal MontoTotalAPagar)
{
    public static IngresoAutomovilConsulta Crear(
        PrqIngresoAutomovil ingreso,
        decimal precioPorHora,
        DateTime ahora)
    {
        var automovil = ingreso.Automovil
            ?? throw new InvalidDataException(
                $"El ingreso {ingreso.Consecutivo} no tiene un automóvil relacionado.");
        var parqueo = ingreso.Parqueo
            ?? throw new InvalidDataException(
                $"El ingreso {ingreso.Consecutivo} no tiene un parqueo relacionado.");
        var fechaFin = ingreso.FechaHoraSalida ?? ahora;
        var horas = (decimal)(fechaFin - ingreso.FechaHoraEntrada).TotalHours;

        return new IngresoAutomovilConsulta(
            ingreso.Consecutivo,
            automovil.Id,
            automovil.Color,
            automovil.Anio,
            automovil.Fabricante,
            automovil.Tipo,
            parqueo.Id,
            parqueo.NombreProvincia,
            parqueo.Nombre,
            ingreso.FechaHoraEntrada,
            ingreso.FechaHoraSalida,
            decimal.Round(horas * precioPorHora, 2, MidpointRounding.AwayFromZero));
    }
}
