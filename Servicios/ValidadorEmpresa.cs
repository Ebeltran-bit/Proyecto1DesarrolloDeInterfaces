using System.Text.RegularExpressions;
using AgendaConsultora.Modelos;

namespace AgendaConsultora.Servicios;

// Reglas propias de los datos de una empresa.
public static class ValidadorEmpresa
{
    // Letra de tipo de entidad + 7 dígitos + dígito o letra de control (ej. B12345678).
    private static readonly Regex FormatoCif = new(@"^[A-HJNP-SUVW]\d{7}[0-9A-J]$");

    public static string NormalizarCif(string valor) =>
        Validador.QuitarSeparadores(valor).ToUpperInvariant();

    public static string? ValidarCif(string valor)
    {
        if (valor == "")
            return "CIF: es obligatorio.";
        if (!FormatoCif.IsMatch(valor))
            return "CIF: formato esperado letra + 7 dígitos + dígito o letra de control (ej. B12345678).";
        return null;
    }

    public static void Normalizar(Empresa empresa)
    {
        empresa.NombreComercial = Validador.NormalizarTexto(empresa.NombreComercial);
        empresa.Cif = NormalizarCif(empresa.Cif);
        empresa.Telefono = Validador.NormalizarTelefono(empresa.Telefono);
        empresa.Correo = Validador.NormalizarCorreo(empresa.Correo);
        empresa.Direccion = Validador.NormalizarTexto(empresa.Direccion);
    }

    public static List<string> Validar(Empresa empresa) => Validador.Reunir(
        Validador.ValidarObligatorio(empresa.NombreComercial, "Nombre comercial"),
        ValidarCif(empresa.Cif),
        Validador.ValidarTelefono(empresa.Telefono),
        Validador.ValidarCorreo(empresa.Correo),
        Validador.ValidarObligatorio(empresa.Direccion, "Dirección"));
}
