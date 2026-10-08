using AgendaConsultora.Datos;
using AgendaConsultora.Modelos;
using AgendaConsultora.Utilidades;

namespace AgendaConsultora.Servicios;

// Lógica de negocio de las empresas: validación, CIF único, baja lógica (Estado 0),
// recuperación (Estado 1) y comprobación de personas vinculadas.
public class ServicioEmpresas
{
    private readonly IRepositorioEmpresas repositorio;
    private readonly IRepositorioPersonas repositorioPersonas;

    public ServicioEmpresas(IRepositorioEmpresas repositorio, IRepositorioPersonas repositorioPersonas)
    {
        this.repositorio = repositorio;
        this.repositorioPersonas = repositorioPersonas;
    }

    // ---------- Consultas ----------

    public List<Empresa> ObtenerActivas() => ConEstado(repositorio.ObtenerTodas(), EstadoEmpresa.Activa);

    public List<Empresa> ObtenerDeBaja() => ConEstado(repositorio.ObtenerTodas(), EstadoEmpresa.Baja);

    public Empresa? ObtenerActiva(int id)
    {
        Empresa? empresa = repositorio.ObtenerPorId(id);
        return empresa != null && empresa.EstaActiva ? empresa : null;
    }

    // Las búsquedas normales solo ven empresas activas; las dadas de baja no se enseñan.
    public List<Empresa> Buscar(string criterio) => Buscar(criterio, EstadoEmpresa.Activa);

    public List<Empresa> BuscarDeBaja(string criterio) => Buscar(criterio, EstadoEmpresa.Baja);

    public List<Persona> ObtenerPersonas(int idEmpresa) => repositorioPersonas.ObtenerPorEmpresa(idEmpresa);

    // El CIF no puede repetirse, tampoco con el de una empresa dada de baja.
    // idPropio es el de la empresa que se está editando (0 en un alta).
    public string? ComprobarCifLibre(string cif, int idPropio)
    {
        Empresa? duenia = repositorio.ObtenerPorCif(cif);

        if (duenia == null || duenia.Id == idPropio)
            return null;

        string aviso = duenia.EstaActiva ? "" : " (está dada de baja: puedes recuperarla)";
        return $"CIF: ya pertenece a {duenia.Codigo} - {duenia.NombreComercial}{aviso}.";
    }

    // ---------- Operaciones ----------

    public Resultado DarDeAlta(Empresa empresa)
    {
        empresa.Estado = EstadoEmpresa.Activa;
        Resultado resultado = Validar(empresa);

        if (resultado.Correcto)
            repositorio.Agregar(empresa);

        return resultado;
    }

    public Resultado Modificar(Empresa empresa)
    {
        if (ObtenerActiva(empresa.Id) == null)
            return Resultado.Error($"No existe la empresa {empresa.Codigo}.");

        Resultado resultado = Validar(empresa);

        if (resultado.Correcto)
            repositorio.Actualizar(empresa);

        return resultado;
    }

    // Baja lógica: solo se permite si ninguna persona está vinculada a la empresa.
    public Resultado DarDeBaja(int id)
    {
        Empresa? empresa = ObtenerActiva(id);
        if (empresa == null)
            return Resultado.Error($"No existe ninguna empresa activa con Id {CodigoId.Formatear(Empresa.PrefijoId, id)}.");

        int vinculadas = ObtenerPersonas(id).Count;
        if (vinculadas > 0)
            return Resultado.Error($"{empresa.Codigo} no se puede dar de baja: tiene {vinculadas} persona(s) vinculada(s).");

        return CambiarEstado(empresa, EstadoEmpresa.Baja);
    }

    public Resultado Recuperar(int id)
    {
        Empresa? empresa = repositorio.ObtenerPorId(id);
        if (empresa == null || empresa.EstaActiva)
            return Resultado.Error($"No existe ninguna empresa dada de baja con Id {CodigoId.Formatear(Empresa.PrefijoId, id)}.");

        return CambiarEstado(empresa, EstadoEmpresa.Activa);
    }

    // ---------- Funciones auxiliares ----------

    private Resultado CambiarEstado(Empresa empresa, EstadoEmpresa estado)
    {
        empresa.Estado = estado;
        repositorio.Actualizar(empresa);
        return Resultado.Ok();
    }

    // Si el criterio tiene forma de código (E3, e003) busca por Id;
    // si no, busca el texto en el nombre comercial o el CIF.
    private List<Empresa> Buscar(string criterio, EstadoEmpresa estado)
    {
        var encontradas = new List<Empresa>();

        if (CodigoId.IntentarLeer(criterio, Empresa.PrefijoId, out int id))
        {
            Empresa? empresa = repositorio.ObtenerPorId(id);
            if (empresa != null)
                encontradas.Add(empresa);
        }
        else
        {
            encontradas = repositorio.BuscarPorTexto(criterio.Trim());
        }

        return ConEstado(encontradas, estado);
    }

    private Resultado Validar(Empresa empresa)
    {
        ValidadorEmpresa.Normalizar(empresa);

        List<string> errores = ValidadorEmpresa.Validar(empresa);
        errores.AddRange(Validador.Reunir(ComprobarCifLibre(empresa.Cif, empresa.Id)));

        return Resultado.Desde(errores);
    }

    private static List<Empresa> ConEstado(IEnumerable<Empresa> empresas, EstadoEmpresa estado) =>
        empresas.Where(e => e.Estado == estado)
                .OrderBy(e => e.NombreComercial, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
}
