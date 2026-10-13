using AgendaProfesional.Infrastructure;
using AgendaProfesional.Models;
using AgendaProfesional.Utils;

namespace AgendaProfesional.Services;

// Operaciones de negocio de la agenda: altas, búsquedas, modificaciones, bajas,
// asignaciones y consultas. Aplica las reglas (validación, correo y CIF únicos,
// empresa activa, no borrar empresas con personas) antes de tocar los datos.
// No escribe nada en pantalla y no sabe si los datos van a un JSON o a una base de datos.
public class AgendaService
{
    private readonly IRepositorioPersonas personas;
    private readonly IRepositorioEmpresas empresas;

    public AgendaService(IRepositorioPersonas personas, IRepositorioEmpresas empresas)
    {
        this.personas = personas;
        this.empresas = empresas;
    }

    public bool EstaVacia() => personas.ObtenerTodas().Count == 0 && empresas.ObtenerTodas().Count == 0;

    // ============================================================
    //  Personas
    // ============================================================

    public List<Persona> ObtenerPersonas() => OrdenarPorApellidos(personas.ObtenerTodas());

    // Por código (P3, p003) o por texto en nombre y apellidos.
    public List<Persona> BuscarPersonas(string criterio) =>
        OrdenarPorApellidos(Busqueda.PorCodigoOTexto<Persona>(
            criterio, Persona.PrefijoId, personas.ObtenerPorId, personas.BuscarPorNombre));

    // idPropio es el de la persona que se está editando (0 en un alta),
    // para que su propio correo no cuente como repetido.
    public string? ComprobarCorreoLibre(string correo, int idPropio)
    {
        Persona? duenio = personas.ObtenerPorCorreo(correo);

        if (duenio == null || duenio.Id == idPropio)
            return null;

        return $"Correo: ya pertenece a {duenio.Codigo} - {duenio.NombreCompleto}.";
    }

    // Una persona solo se puede vincular a una empresa que exista y esté activa.
    public string? ComprobarEmpresaAsignable(int? idEmpresa)
    {
        if (idEmpresa == null || ObtenerEmpresaActiva(idEmpresa.Value) != null)
            return null;

        return $"Empresa: no existe ninguna empresa activa con Id {CodigoEmpresa(idEmpresa.Value)}.";
    }

    public Resultado AltaPersona(Persona persona)
    {
        Resultado resultado = ValidarPersona(persona);

        if (resultado.Correcto)
            personas.Agregar(persona);

        return resultado;
    }

    public Resultado ModificarPersona(Persona persona)
    {
        if (personas.ObtenerPorId(persona.Id) == null)
            return Resultado.Error($"No existe la persona {persona.Codigo}.");

        Resultado resultado = ValidarPersona(persona);

        if (resultado.Correcto)
            personas.Actualizar(persona);

        return resultado;
    }

    public bool EliminarPersona(int id) => personas.Eliminar(id);

    // ============================================================
    //  Empresas
    // ============================================================

    // Solo las activas (Estado 1): las dadas de baja no se enseñan.
    public List<Empresa> ObtenerEmpresasActivas() => FiltrarPorEstado(empresas.ObtenerTodas(), EstadoEmpresa.Activa);

    public List<Empresa> ObtenerEmpresasDeBaja() => FiltrarPorEstado(empresas.ObtenerTodas(), EstadoEmpresa.Baja);

    public Empresa? ObtenerEmpresaActiva(int id)
    {
        Empresa? empresa = empresas.ObtenerPorId(id);
        return empresa != null && empresa.EstaActiva ? empresa : null;
    }

    public List<Empresa> BuscarEmpresas(string criterio) => BuscarEmpresas(criterio, EstadoEmpresa.Activa);

    public List<Empresa> BuscarEmpresasDeBaja(string criterio) => BuscarEmpresas(criterio, EstadoEmpresa.Baja);

    // El CIF no puede repetirse, tampoco con el de una empresa dada de baja.
    // idPropio es el de la empresa que se está editando (0 en un alta).
    public string? ComprobarCifLibre(string cif, int idPropio)
    {
        Empresa? duenia = empresas.ObtenerPorCif(cif);

        if (duenia == null || duenia.Id == idPropio)
            return null;

        string aviso = duenia.EstaActiva ? "" : " (está dada de baja: puedes recuperarla)";
        return $"CIF: ya pertenece a {duenia.Codigo} - {duenia.NombreComercial}{aviso}.";
    }

    public Resultado AltaEmpresa(Empresa empresa)
    {
        empresa.Estado = EstadoEmpresa.Activa;
        Resultado resultado = ValidarEmpresa(empresa);

        if (resultado.Correcto)
            empresas.Agregar(empresa);

        return resultado;
    }

    public Resultado ModificarEmpresa(Empresa empresa)
    {
        if (ObtenerEmpresaActiva(empresa.Id) == null)
            return Resultado.Error($"No existe la empresa {empresa.Codigo}.");

        Resultado resultado = ValidarEmpresa(empresa);

        if (resultado.Correcto)
            empresas.Actualizar(empresa);

        return resultado;
    }

    // Baja lógica (Estado 0). Política: no se permite mientras tenga personas vinculadas.
    public Resultado DarDeBajaEmpresa(int id)
    {
        Empresa? empresa = ObtenerEmpresaActiva(id);
        if (empresa == null)
            return Resultado.Error($"No existe ninguna empresa activa con Id {CodigoEmpresa(id)}.");

        int vinculadas = personas.ObtenerPorEmpresa(id).Count;
        if (vinculadas > 0)
            return Resultado.Error($"{empresa.Codigo} no se puede dar de baja: tiene {vinculadas} persona(s) vinculada(s).");

        return CambiarEstado(empresa, EstadoEmpresa.Baja);
    }

    public Resultado RecuperarEmpresa(int id)
    {
        Empresa? empresa = empresas.ObtenerPorId(id);
        if (empresa == null || empresa.EstaActiva)
            return Resultado.Error($"No existe ninguna empresa dada de baja con Id {CodigoEmpresa(id)}.");

        return CambiarEstado(empresa, EstadoEmpresa.Activa);
    }

    // ============================================================
    //  Relación persona - empresa
    // ============================================================

    // Plantilla de una empresa, ordenada por apellidos.
    public List<Persona> ObtenerPersonasDeEmpresa(int idEmpresa) =>
        OrdenarPorApellidos(personas.ObtenerPorEmpresa(idEmpresa));

    public List<Persona> ObtenerPersonasSinEmpresa() =>
        OrdenarPorApellidos(personas.ObtenerTodas().Where(p => p.IdEmpresa == null));

    // Asigna o cambia la empresa de una persona. Ambas deben existir y la empresa estar activa.
    public Resultado AsignarEmpresa(int idPersona, int idEmpresa)
    {
        Persona? persona = personas.ObtenerPorId(idPersona);
        if (persona == null)
            return Resultado.Error($"No existe la persona {CodigoPersona(idPersona)}.");

        persona.IdEmpresa = idEmpresa;
        return ModificarPersona(persona);
    }

    // La persona conserva todos sus datos y se queda sin empresa asignada.
    public Resultado DesvincularPersona(int idPersona)
    {
        Persona? persona = personas.ObtenerPorId(idPersona);
        if (persona == null)
            return Resultado.Error($"No existe la persona {CodigoPersona(idPersona)}.");

        if (persona.IdEmpresa == null)
            return Resultado.Error($"{persona.Codigo} no tiene ninguna empresa asignada.");

        persona.IdEmpresa = null;
        return ModificarPersona(persona);
    }

    // ============================================================
    //  Funciones internas
    // ============================================================

    private Resultado ValidarPersona(Persona persona)
    {
        InputValidator.Normalizar(persona);

        List<string> errores = InputValidator.Validar(persona);
        errores.AddRange(InputValidator.Reunir(
            ComprobarCorreoLibre(persona.Correo, persona.Id),
            ComprobarEmpresaAsignable(persona.IdEmpresa)));

        return Resultado.Desde(errores);
    }

    private Resultado ValidarEmpresa(Empresa empresa)
    {
        InputValidator.Normalizar(empresa);

        List<string> errores = InputValidator.Validar(empresa);
        errores.AddRange(InputValidator.Reunir(ComprobarCifLibre(empresa.Cif, empresa.Id)));

        return Resultado.Desde(errores);
    }

    private Resultado CambiarEstado(Empresa empresa, EstadoEmpresa estado)
    {
        empresa.Estado = estado;
        empresas.Actualizar(empresa);
        return Resultado.Ok();
    }

    private List<Empresa> BuscarEmpresas(string criterio, EstadoEmpresa estado) =>
        FiltrarPorEstado(Busqueda.PorCodigoOTexto<Empresa>(
            criterio, Empresa.PrefijoId, empresas.ObtenerPorId, empresas.BuscarPorTexto), estado);

    private static List<Empresa> FiltrarPorEstado(IEnumerable<Empresa> lista, EstadoEmpresa estado) =>
        lista.Where(e => e.Estado == estado)
             .OrderBy(e => e.NombreComercial, StringComparer.CurrentCultureIgnoreCase)
             .ToList();

    private static List<Persona> OrdenarPorApellidos(IEnumerable<Persona> lista) =>
        lista.OrderBy(p => p.Apellidos, StringComparer.CurrentCultureIgnoreCase)
             .ThenBy(p => p.Nombre, StringComparer.CurrentCultureIgnoreCase)
             .ToList();

    private static string CodigoPersona(int id) => CodigoId.Formatear(Persona.PrefijoId, id);

    private static string CodigoEmpresa(int id) => CodigoId.Formatear(Empresa.PrefijoId, id);
}
