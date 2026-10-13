using AgendaProfesional.Models;

namespace AgendaProfesional.Infrastructure;

// Contratos de acceso a datos. AgendaService solo conoce estas interfaces, así que
// cambiar el JSON por una base de datos no obliga a tocar el servicio ni la consola.

public interface IRepositorio<T> where T : class, IEntidad<T>
{
    List<T> ObtenerTodas();

    T? ObtenerPorId(int id);

    // Asigna un Id nuevo al elemento recibido y lo guarda.
    void Agregar(T elemento);

    void Actualizar(T elemento);

    bool Eliminar(int id);
}

public interface IRepositorioPersonas : IRepositorio<Persona>
{
    Persona? ObtenerPorCorreo(string correo);

    List<Persona> BuscarPorNombre(string texto);

    // Personas vinculadas a una empresa (el lado "N" de la relación).
    List<Persona> ObtenerPorEmpresa(int idEmpresa);
}

// Devuelve empresas en cualquier estado; decidir cuáles se muestran es cosa del servicio.
public interface IRepositorioEmpresas : IRepositorio<Empresa>
{
    Empresa? ObtenerPorCif(string cif);

    // Busca el texto en el nombre comercial o en el CIF.
    List<Empresa> BuscarPorTexto(string texto);
}
