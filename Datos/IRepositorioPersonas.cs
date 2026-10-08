using AgendaConsultora.Modelos;

namespace AgendaConsultora.Datos;

public interface IRepositorioPersonas : IRepositorio<Persona>
{
    Persona? ObtenerPorCorreo(string correo);

    List<Persona> BuscarPorNombre(string texto);

    // Personas vinculadas a una empresa (el lado "N" de la relación).
    List<Persona> ObtenerPorEmpresa(int idEmpresa);
}
