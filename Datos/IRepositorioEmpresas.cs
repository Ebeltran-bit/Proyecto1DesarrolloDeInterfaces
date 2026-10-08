using AgendaConsultora.Modelos;

namespace AgendaConsultora.Datos;

// Devuelve empresas en cualquier estado; decidir cuáles se muestran es cosa del servicio.
public interface IRepositorioEmpresas : IRepositorio<Empresa>
{
    Empresa? ObtenerPorCif(string cif);

    // Busca el texto en el nombre comercial o en el CIF.
    List<Empresa> BuscarPorTexto(string texto);
}
