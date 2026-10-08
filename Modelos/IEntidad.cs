namespace AgendaConsultora.Modelos;

// Lo que tienen en común Persona y Empresa. Gracias a esta interfaz, el repositorio
// en memoria y los menús se escriben una sola vez y sirven para las dos.
public interface IEntidad<T>
{
    // Lo asigna el repositorio al guardar. 0 = todavía no guardada.
    int Id { get; set; }

    // Código que ve el usuario (P001, E001). Internamente se trabaja con el Id numérico.
    string Codigo { get; }

    T Clonar();
}
