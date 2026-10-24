namespace ParqueoMvc.Repositories.Json;

internal static class TextoAproximado
{
    public static bool Coincide(string valor, string? criterio) =>
        string.IsNullOrWhiteSpace(criterio)
        || valor.Contains(criterio.Trim(), StringComparison.OrdinalIgnoreCase);
}
