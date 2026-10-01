using AgendaConsultora.Utilidades;

namespace AgendaConsultora.Modelos;

// Persona de contacto de la agenda. Solo contiene datos: las reglas están en ValidadorPersona
// y el acceso a los datos en el repositorio.
public class Persona
{
    public const string PrefijoId = "P";

    // Lo asigna el repositorio al guardar (en la Fase 2, la base de datos). 0 = todavía no guardada.
    public int Id { get; set; }

    public string Nombre { get; set; } = "";
    public string Apellidos { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Correo { get; set; } = "";

    // Texto libre en la Fase 1. En la Fase 2 se sustituirá por IdEmpresa.
    public string Empresa { get; set; } = "";

    // Campo añadido: rol de la persona dentro de su empresa.
    public string Cargo { get; set; } = "";

    // Código que ve el usuario (P001). Internamente se trabaja con el Id numérico.
    public string Codigo => CodigoId.Formatear(PrefijoId, Id);

    public string NombreCompleto => $"{Nombre} {Apellidos}";

    public Persona Clonar() => (Persona)MemberwiseClone();
}
