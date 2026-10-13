using System.Text.RegularExpressions;
using AgendaProfesional.Models;

namespace AgendaProfesional.Utils;

// Reglas de formato de los datos. No lee ni escribe nada: recibe un valor y devuelve
// el mensaje de error, o null si es correcto. La usan la consola (al teclear cada dato)
// y AgendaService (antes de guardar), así las reglas están en un único sitio.
public static class InputValidator
{
    private static readonly Regex FormatoTelefono = new(@"^\+?\d{9,15}$");
    private static readonly Regex FormatoCorreo = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    // Letra de tipo de entidad + 7 dígitos + dígito o letra de control (ej. B12345678).
    private static readonly Regex FormatoCif = new(@"^[A-HJNP-SUVW]\d{7}[0-9A-J]$");

    // ------------------------------------------------------------
    //  Normalización: cómo se guarda cada dato
    // ------------------------------------------------------------

    public static string NormalizarTexto(string valor) => valor.Trim();

    // Se aceptan espacios y guiones al escribir, pero se guarda solo el número.
    public static string NormalizarTelefono(string valor) => QuitarSeparadores(valor);

    public static string NormalizarCorreo(string valor) => valor.Trim().ToLowerInvariant();

    public static string NormalizarCif(string valor) => QuitarSeparadores(valor).ToUpperInvariant();

    // ------------------------------------------------------------
    //  Validación de un dato
    // ------------------------------------------------------------

    public static string? ValidarObligatorio(string valor, string campo) =>
        valor == "" ? $"{campo}: es obligatorio." : null;

    public static string? ValidarNombre(string valor, string campo)
    {
        if (valor.Any(char.IsDigit))
            return $"{campo}: no puede contener números.";

        return ValidarObligatorio(valor, campo);
    }

    public static string? ValidarTelefono(string valor) =>
        ValidarFormato(valor, "Teléfono", FormatoTelefono,
                       "solo dígitos (opcionalmente con + delante), entre 9 y 15.");

    public static string? ValidarCorreo(string valor) =>
        ValidarFormato(valor, "Correo", FormatoCorreo, "formato esperado usuario@dominio.ext");

    public static string? ValidarCif(string valor) =>
        ValidarFormato(valor, "CIF", FormatoCif,
                       "formato esperado letra + 7 dígitos + dígito o letra de control (ej. B12345678).");

    // ------------------------------------------------------------
    //  Registro completo
    // ------------------------------------------------------------

    public static void Normalizar(Persona persona)
    {
        persona.Nombre = NormalizarTexto(persona.Nombre);
        persona.Apellidos = NormalizarTexto(persona.Apellidos);
        persona.Telefono = NormalizarTelefono(persona.Telefono);
        persona.Correo = NormalizarCorreo(persona.Correo);
        persona.Cargo = NormalizarTexto(persona.Cargo);
    }

    public static List<string> Validar(Persona persona) => Reunir(
        ValidarNombre(persona.Nombre, "Nombre"),
        ValidarNombre(persona.Apellidos, "Apellidos"),
        ValidarTelefono(persona.Telefono),
        ValidarCorreo(persona.Correo));

    public static void Normalizar(Empresa empresa)
    {
        empresa.NombreComercial = NormalizarTexto(empresa.NombreComercial);
        empresa.Cif = NormalizarCif(empresa.Cif);
        empresa.Telefono = NormalizarTelefono(empresa.Telefono);
        empresa.Correo = NormalizarCorreo(empresa.Correo);
        empresa.Direccion = NormalizarTexto(empresa.Direccion);
    }

    public static List<string> Validar(Empresa empresa) => Reunir(
        ValidarObligatorio(empresa.NombreComercial, "Nombre comercial"),
        ValidarCif(empresa.Cif),
        ValidarTelefono(empresa.Telefono),
        ValidarCorreo(empresa.Correo),
        ValidarObligatorio(empresa.Direccion, "Dirección"));

    // Junta los errores encontrados descartando los null (campos correctos).
    public static List<string> Reunir(params string?[] errores) => errores.OfType<string>().ToList();

    // ------------------------------------------------------------
    //  Funciones internas
    // ------------------------------------------------------------

    private static string? ValidarFormato(string valor, string campo, Regex formato, string explicacion)
    {
        if (valor == "")
            return $"{campo}: es obligatorio.";
        if (!formato.IsMatch(valor))
            return $"{campo}: {explicacion}";
        return null;
    }

    private static string QuitarSeparadores(string valor) =>
        valor.Replace(" ", "").Replace("-", "").Trim();
}
