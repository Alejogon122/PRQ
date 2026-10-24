namespace ParqueoMvc.Repositories.Models;

internal static class ConsultaValidator
{
    public static void ValidarRango(DateTime desde, DateTime hasta)
    {
        if (desde > hasta)
        {
            throw new ArgumentException("La fecha inicial no puede ser posterior a la fecha final.");
        }
    }

    public static void ValidarRango<T>(T? desde, T? hasta, string nombreCampo)
        where T : struct, IComparable<T>
    {
        if (desde.HasValue && hasta.HasValue && desde.Value.CompareTo(hasta.Value) > 0)
        {
            throw new ArgumentException($"El límite inicial de {nombreCampo} no puede superar al límite final.");
        }
    }
}
