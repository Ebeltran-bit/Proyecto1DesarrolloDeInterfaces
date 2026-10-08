using AgendaConsultora.Modelos;
using AgendaConsultora.Utilidades;

namespace AgendaConsultora.Datos;

public class RepositorioEmpresasMemoria : RepositorioMemoria<Empresa>, IRepositorioEmpresas
{
    public Empresa? ObtenerPorCif(string cif) =>
        Primero(e => e.Cif.Equals(cif, StringComparison.OrdinalIgnoreCase));

    public List<Empresa> BuscarPorTexto(string texto) =>
        Filtrar(e => Texto.Contiene(e.NombreComercial, texto) || Texto.Contiene(e.Cif, texto));
}
