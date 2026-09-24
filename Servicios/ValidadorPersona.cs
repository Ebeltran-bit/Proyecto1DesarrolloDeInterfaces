using System.Text.RegularExpressions;
using AgendaConsultora.Modelos;

namespace AgendaConsultora.Servicios;

// Reglas de formato de cada dato de una persona. Cada función devuelve el mensaje
// de error, o null si el valor es correcto.
public static class ValidadorPersona
{
    private static readonly Regex FormatoTelefono = new(@"^\+?\d{9,15}$");
    private static readonly Regex FormatoCorreo = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    // --- Normalización: cómo se guarda cada dato ---

    public static string NormalizarTexto(string valor) => valor.Trim();

    // Se aceptan espacios y guiones al escribir, pero se guarda solo el número.
    public static string NormalizarTelefono(string valor) =>
        valor.Replace(" ", "").Replace("-", "").Trim();

    public static string NormalizarCorreo(string valor) => valor.Trim().ToLowerInvariant();

    // --- Validación de cada campo ---

    public static string? ValidarNombre(string valor, string campo)
    {
        if (valor == "")
            return $"{campo}: es obligatorio.";
        if (valor.Any(char.IsDigit))
            return $"{campo}: no puede contener números.";
        return null;
    }

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

    // --- Persona completa ---

    public static void Normalizar(Persona persona)
    {
        persona.Nombre = NormalizarTexto(persona.Nombre);
        persona.Apellidos = NormalizarTexto(persona.Apellidos);
        persona.Telefono = NormalizarTelefono(persona.Telefono);
        persona.Correo = NormalizarCorreo(persona.Correo);
        persona.Empresa = NormalizarTexto(persona.Empresa);
        persona.Cargo = NormalizarTexto(persona.Cargo);
    }

    public static List<string> Validar(Persona persona)
    {
        string?[] errores =
        {
            ValidarNombre(persona.Nombre, "Nombre"),
            ValidarNombre(persona.Apellidos, "Apellidos"),
            ValidarTelefono(persona.Telefono),
            ValidarCorreo(persona.Correo),
        };

        return errores.OfType<string>().ToList();
    }
}
