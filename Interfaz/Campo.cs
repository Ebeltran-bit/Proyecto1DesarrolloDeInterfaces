namespace AgendaConsultora.Interfaz;

// Describe un dato editable en un único sitio: cómo se llama, si es obligatorio,
// su ancho en la tabla y cómo se lee, guarda, normaliza y valida.
// El alta, la ficha, la tabla y el menú de modificar recorren la misma lista de campos,
// así que añadir un dato nuevo es añadir una línea.
public record Campo<T>(
    string Etiqueta,
    bool Obligatorio,
    int AnchoColumna,
    Func<T, string> Obtener,
    Action<T, string> Asignar,
    Func<string, string> Normalizar,
    Func<string, T, string?> Validar)
{
    // Texto que se enseña cuando no coincide con el valor que se escribe
    // (se escribe el Id de la empresa, pero se enseña su nombre).
    public Func<T, string>? Mostrar { get; init; }

    // Información que se muestra justo antes de pedir el valor.
    public Action? Ayuda { get; init; }

    public string TextoVisible(T entidad) => Pantalla.TextoOGuion((Mostrar ?? Obtener)(entidad));

    // Para los campos opcionales de texto libre: cualquier valor es correcto.
    public static string? SinValidar(string valor, T entidad) => null;
}
