using AgendaConsultora.Modelos;

namespace AgendaConsultora.Datos;

// Todo lo que el programa necesita para guardar y recuperar personas.
// El resto del código solo conoce esta interfaz, así que en la Fase 2 se podrá añadir
// una implementación con base de datos sin tocar servicios ni menús.
public interface IRepositorioPersonas
{
    List<Persona> ObtenerTodas();

    Persona? ObtenerPorId(int id);

    Persona? ObtenerPorCorreo(string correo);

    List<Persona> BuscarPorNombre(string texto);

    // Asigna un Id nuevo a la persona recibida y la guarda.
    void Agregar(Persona persona);

    void Actualizar(Persona persona);

    bool Eliminar(int id);
}
