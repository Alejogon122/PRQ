using ParqueoMvc.Models.PRQ;

namespace ParqueoMvc.Repositories.Models;

internal static class EntidadValidator
{
    public static void Validar(PrqAutomovil automovil)
    {
        ArgumentNullException.ThrowIfNull(automovil);
        ValidarLongitud(automovil.Color, 50, nameof(automovil.Color));
        ValidarLongitud(automovil.Fabricante, 100, nameof(automovil.Fabricante));

        if (automovil.Anio <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(automovil.Anio), "El año debe ser mayor que cero.");
        }

        if (automovil.Tipo is not ("sedán" or "4x4" or "moto"))
        {
            throw new ArgumentException("El tipo debe ser 'sedán', '4x4' o 'moto'.", nameof(automovil.Tipo));
        }
    }

    public static void Validar(PrqParqueo parqueo)
    {
        ArgumentNullException.ThrowIfNull(parqueo);
        ValidarLongitud(parqueo.NombreProvincia, 100, nameof(parqueo.NombreProvincia));
        ValidarLongitud(parqueo.Nombre, 150, nameof(parqueo.Nombre));

        if (parqueo.PrecioPorHora < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parqueo.PrecioPorHora),
                "El precio por hora no puede ser negativo.");
        }
    }

    public static void Validar(PrqIngresoAutomovil ingreso)
    {
        ArgumentNullException.ThrowIfNull(ingreso);

        if (ingreso.FechaHoraSalida < ingreso.FechaHoraEntrada)
        {
            throw new ArgumentException(
                "La fecha y hora de salida no puede ser anterior a la entrada.",
                nameof(ingreso.FechaHoraSalida));
        }
    }

    public static void ValidarClaveNueva(int id, string nombreClave)
    {
        if (id != 0)
        {
            throw new ArgumentException(
                "La llave primaria se genera automáticamente al insertar.",
                nombreClave);
        }
    }

    private static void ValidarLongitud(string? valor, int longitudMaxima, string nombreCampo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException("El campo es obligatorio.", nombreCampo);
        }

        if (valor.Length > longitudMaxima)
        {
            throw new ArgumentException(
                $"El campo no puede superar {longitudMaxima} caracteres.",
                nombreCampo);
        }
    }
}
