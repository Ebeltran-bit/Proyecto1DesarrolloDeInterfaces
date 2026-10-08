using System.Text.RegularExpressions;

namespace AgendaConsultora.Servicios;

// Reglas de formato compartidas por personas y empresas. Cada función de validación
// devuelve el mensaje de error, o null si el valor es correcto.
public static class Validador
{
    private static readonly Regex FormatoTelefono = new(@"^\+?\d{9,15}$");
    private static readonly Regex FormatoCorreo = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    // --- Normalización: cómo se guarda cada dato ---

    public static string NormalizarTexto(string valor) => valor.Trim();

    // Se aceptan espacios y guiones al escribir, pero se guarda solo el número.
    public static string NormalizarTelefono(string valor) => QuitarSeparadores(valor);

    public static string NormalizarCorreo(string valor) => valor.Trim().ToLowerInvariant();

    public static string QuitarSeparadores(string valor) =>
        valor.Replace(" ", "").Replace("-", "").Trim();

    // --- Validación ---

    public static string? ValidarObligatorio(string valor, string campo) =>
        valor == "" ? $"{campo}: es obligatorio." : null;

    public static string? ValidarTelefono(string valor)
    {
        if (valor == "")
            return "Teléfono: es obligatorio.";
        if (!FormatoTelefono.IsMatch(valor))
            return "Teléfono: solo dígitos (opcionalmente con + delante), entre 9 y 15.";
        return null;
    }

    public static string? ValidarCorreo(string valor)
    {
        if (valor == "")
            return "Correo: es obligatorio.";
        if (!FormatoCorreo.IsMatch(valor))
            return "Correo: formato esperado usuario@dominio.ext";
        return null;
    }

    // Junta los errores encontrados descartando los null (campos correctos).
    public static List<string> Reunir(params string?[] errores) => errores.OfType<string>().ToList();
}
