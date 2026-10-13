using AgendaProfesional.Models;
using AgendaProfesional.Utils;

namespace AgendaProfesional.Infrastructure;

public class RepositorioPersonas : RepositorioBase<Persona>, IRepositorioPersonas
{
    public RepositorioPersonas(ContextoAgenda contexto) : base(contexto)
    {
    }

    protected override List<Persona> Elementos => Contexto.Datos.Personas;

    protected override int SiguienteId() => Contexto.SiguienteIdPersona();

    public Persona? ObtenerPorCorreo(string correo) =>
        Primero(p => p.Correo.Equals(correo, StringComparison.OrdinalIgnoreCase));

    public List<Persona> BuscarPorNombre(string texto) =>
        Filtrar(p => Texto.Contiene(p.NombreCompleto, texto));

    public List<Persona> ObtenerPorEmpresa(int idEmpresa) =>
        Filtrar(p => p.IdEmpresa == idEmpresa);
}
