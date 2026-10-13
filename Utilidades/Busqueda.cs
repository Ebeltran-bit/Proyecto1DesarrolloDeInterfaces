namespace AgendaConsultora.Utilidades;

// Búsqueda común a personas y empresas, para no repetirla en cada servicio.
public static class Busqueda
{
    // Si el criterio tiene forma de código (P3, e003) busca por Id; si no, busca por texto.
    public static List<T> PorCodigoOTexto<T>(string criterio, string prefijo,
                                             Func<int, T?> buscarPorId,
                                             Func<string, List<T>> buscarPorTexto) where T : class
    {
        if (CodigoId.IntentarLeer(criterio, prefijo, out int id))
        {
            T? encontrado = buscarPorId(id);
            return encontrado == null ? new List<T>() : new List<T> { encontrado };
        }

        return buscarPorTexto(criterio.Trim());
    }
}
