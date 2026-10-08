namespace AgendaConsultora.Utilidades;

// Convierte entre el Id numérico interno y el código que ve el usuario
// (P001 para personas, E001 para empresas).
public static class CodigoId
{
    public static string Formatear(string prefijo, int id) => $"{prefijo}{id:D3}";

    // Acepta "p3", "P03" o "P003". Devuelve false si el texto no tiene forma de código.
    public static bool IntentarLeer(string texto, string prefijo, out int id)
    {
        id = 0;
        texto = texto.Trim();

        if (!texto.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase))
            return false;

        string numero = texto[prefijo.Length..];
        return numero.Length > 0
            && numero.All(char.IsAsciiDigit)
            && int.TryParse(numero, out id)
            && id > 0;
    }

    // Deja el código en su forma estándar ("e1" -> "E001"). Si no es un código, devuelve el texto tal cual.
    public static string Normalizar(string texto, string prefijo) =>
        IntentarLeer(texto, prefijo, out int id) ? Formatear(prefijo, id) : texto.Trim();
}
