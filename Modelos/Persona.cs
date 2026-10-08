using AgendaConsultora.Utilidades;

namespace AgendaConsultora.Modelos;

// Persona de contacto de la agenda. Solo contiene datos: las reglas están en ValidadorPersona
// y el acceso a los datos en el repositorio.
public class Persona : IEntidad<Persona>
{
    public const string PrefijoId = "P";

    public int Id { get; set; }

    public string Nombre { get; set; } = "";
    public string Apellidos { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Correo { get; set; } = "";

    // Empresa a la que pertenece (null = sin empresa). Varias personas pueden
    // apuntar a la misma empresa: es el lado "N" de la relación 1:N.
    public int? IdEmpresa { get; set; }

    // Campo añadido: rol de la persona dentro de su empresa.
    public string Cargo { get; set; } = "";

    public string Codigo => CodigoId.Formatear(PrefijoId, Id);

    public string NombreCompleto => $"{Nombre} {Apellidos}";

    public Persona Clonar() => (Persona)MemberwiseClone();
}
