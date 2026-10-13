using AgendaProfesional.Models;
using AgendaProfesional.Utils;

namespace AgendaProfesional.Infrastructure;

public class RepositorioEmpresas : RepositorioBase<Empresa>, IRepositorioEmpresas
{
    public RepositorioEmpresas(ContextoAgenda contexto) : base(contexto)
    {
    }

    protected override List<Empresa> Elementos => Contexto.Datos.Empresas;

    protected override int SiguienteId() => Contexto.SiguienteIdEmpresa();

    public Empresa? ObtenerPorCif(string cif) =>
        Primero(e => e.Cif.Equals(cif, StringComparison.OrdinalIgnoreCase));

    public List<Empresa> BuscarPorTexto(string texto) =>
        Filtrar(e => Texto.Contiene(e.NombreComercial, texto) || Texto.Contiene(e.Cif, texto));
}
