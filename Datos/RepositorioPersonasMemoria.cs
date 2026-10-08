using AgendaConsultora.Modelos;
using AgendaConsultora.Utilidades;

namespace AgendaConsultora.Datos;

public class RepositorioPersonasMemoria : RepositorioMemoria<Persona>, IRepositorioPersonas
{
    public Persona? ObtenerPorCorreo(string correo) =>
        Primero(p => p.Correo.Equals(correo, StringComparison.OrdinalIgnoreCase));

    public List<Persona> BuscarPorNombre(string texto) =>
        Filtrar(p => Texto.Contiene(p.NombreCompleto, texto));

    public List<Persona> ObtenerPorEmpresa(int idEmpresa) =>
        Filtrar(p => p.IdEmpresa == idEmpresa);
}
