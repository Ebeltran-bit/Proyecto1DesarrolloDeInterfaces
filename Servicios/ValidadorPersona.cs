using AgendaConsultora.Modelos;

namespace AgendaConsultora.Servicios;

// Reglas propias de los datos de una persona.
public static class ValidadorPersona
{
    public static string? ValidarNombre(string valor, string campo)
    {
        if (valor.Any(char.IsDigit))
            return $"{campo}: no puede contener números.";

        return Validador.ValidarObligatorio(valor, campo);
    }

    public static void Normalizar(Persona persona)
    {
        persona.Nombre = Validador.NormalizarTexto(persona.Nombre);
        persona.Apellidos = Validador.NormalizarTexto(persona.Apellidos);
        persona.Telefono = Validador.NormalizarTelefono(persona.Telefono);
        persona.Correo = Validador.NormalizarCorreo(persona.Correo);
        persona.Cargo = Validador.NormalizarTexto(persona.Cargo);
    }

    public static List<string> Validar(Persona persona) => Validador.Reunir(
        ValidarNombre(persona.Nombre, "Nombre"),
        ValidarNombre(persona.Apellidos, "Apellidos"),
        Validador.ValidarTelefono(persona.Telefono),
        Validador.ValidarCorreo(persona.Correo));
}
