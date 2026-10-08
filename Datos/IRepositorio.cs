using AgendaConsultora.Modelos;

namespace AgendaConsultora.Datos;

// Operaciones de almacenamiento comunes a cualquier entidad.
// El resto del código solo conoce las interfaces, así que al pasar a base de datos
// se añaden nuevas implementaciones sin tocar servicios ni menús.
public interface IRepositorio<T> where T : class, IEntidad<T>
{
    List<T> ObtenerTodas();

    T? ObtenerPorId(int id);

    // Asigna un Id nuevo al elemento recibido y lo guarda.
    void Agregar(T elemento);

    void Actualizar(T elemento);

    bool Eliminar(int id);
}
