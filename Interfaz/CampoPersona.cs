using AgendaConsultora.Modelos;

namespace AgendaConsultora.Interfaz;

// Describe un dato de la persona en un único sitio: cómo se llama, si es obligatorio,
// su ancho en la tabla y cómo se lee, guarda, normaliza y valida.
// El alta, la ficha, la tabla y el menú de modificar recorren la misma lista de campos,
// así que añadir un dato nuevo es añadir una línea.
public record CampoPersona(
    string Etiqueta,
    bool Obligatorio,
    int AnchoColumna,
    Func<Persona, string> Obtener,
    Action<Persona, string> Asignar,
    Func<string, string> Normalizar,
    Func<string, Persona, string?> Validar);
