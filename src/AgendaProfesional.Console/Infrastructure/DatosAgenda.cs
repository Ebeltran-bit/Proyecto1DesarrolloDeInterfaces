using AgendaProfesional.Models;

namespace AgendaProfesional.Infrastructure;

// Todo lo que se guarda en el archivo. Los contadores de Id se guardan aparte para que
// un Id borrado no se reutilice nunca, igual que un autoincremental de base de datos.
public class DatosAgenda
{
    public int UltimoIdPersona { get; set; }
    public int UltimoIdEmpresa { get; set; }
    public List<Persona> Personas { get; set; } = new();
    public List<Empresa> Empresas { get; set; } = new();
}
