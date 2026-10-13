using AgendaProfesional.Models;
using AgendaProfesional.Utils;

namespace AgendaProfesional.Infrastructure;

// Unidad de trabajo: carga los datos al arrancar, comprueba que son coherentes,
// los mantiene en memoria y los vuelve a guardar después de cada cambio.
// Los repositorios trabajan sobre este contexto, como harían con una base de datos.
public class ContextoAgenda
{
    private readonly IAlmacenAgenda almacen;
    private readonly List<string> avisos = new();

    public DatosAgenda Datos { get; }

    // Problemas encontrados al cargar (y lo que se hizo con ellos), para enseñarlos al usuario.
    public IReadOnlyList<string> Avisos => avisos;

    public ContextoAgenda(IAlmacenAgenda almacen)
    {
        this.almacen = almacen;
        Datos = CargarDatos();
        ComprobarConsistencia();
    }

    public int SiguienteIdPersona() => ++Datos.UltimoIdPersona;

    public int SiguienteIdEmpresa() => ++Datos.UltimoIdEmpresa;

    public void GuardarCambios() => almacen.Guardar(Datos);

    // ------------------------------------------------------------
    //  Carga y comprobaciones
    // ------------------------------------------------------------

    private DatosAgenda CargarDatos()
    {
        try
        {
            return almacen.Cargar() ?? new DatosAgenda();
        }
        catch (DatosDanadosException ex)
        {
            avisos.Add(ex.Message);
            return new DatosAgenda();
        }
    }

    // Repara lo que se puede reparar sin perder datos y avisa del resto.
    // Solo es necesario si alguien edita el archivo a mano o se guardó una versión antigua.
    private void ComprobarConsistencia()
    {
        Datos.Personas ??= new List<Persona>();
        Datos.Empresas ??= new List<Empresa>();

        bool reparado = AjustarContadores();
        reparado |= ReasignarIdsRepetidos(Datos.Personas, SiguienteIdPersona, "persona");
        reparado |= ReasignarIdsRepetidos(Datos.Empresas, SiguienteIdEmpresa, "empresa");
        reparado |= QuitarEmpresasInexistentes();
        AvisarDatosNoValidos();

        if (reparado)
            GuardarReparaciones();
    }

    // Los contadores nunca pueden quedar por debajo del Id más alto, o se repetirían Ids.
    private bool AjustarContadores()
    {
        int maxPersona = Datos.Personas.Select(p => p.Id).DefaultIfEmpty(0).Max();
        int maxEmpresa = Datos.Empresas.Select(e => e.Id).DefaultIfEmpty(0).Max();
        bool cambiado = false;

        if (Datos.UltimoIdPersona < maxPersona) { Datos.UltimoIdPersona = maxPersona; cambiado = true; }
        if (Datos.UltimoIdEmpresa < maxEmpresa) { Datos.UltimoIdEmpresa = maxEmpresa; cambiado = true; }

        return cambiado;
    }

    private bool ReasignarIdsRepetidos<T>(List<T> elementos, Func<int> siguienteId, string tipo)
        where T : class, IEntidad<T>
    {
        var vistos = new HashSet<int>();
        bool cambiado = false;

        foreach (T elemento in elementos)
        {
            if (elemento.Id > 0 && vistos.Add(elemento.Id))
                continue;

            int anterior = elemento.Id;
            elemento.Id = siguienteId();
            vistos.Add(elemento.Id);
            avisos.Add($"Había una {tipo} con Id repetido o no válido ({anterior}); ahora es {elemento.Codigo}.");
            cambiado = true;
        }

        return cambiado;
    }

    // Una persona no puede apuntar a una empresa que no existe o que está dada de baja.
    private bool QuitarEmpresasInexistentes()
    {
        var activas = Datos.Empresas.Where(e => e.EstaActiva).Select(e => e.Id).ToHashSet();
        bool cambiado = false;

        foreach (Persona persona in Datos.Personas.Where(p => p.IdEmpresa != null && !activas.Contains(p.IdEmpresa.Value)))
        {
            avisos.Add($"{persona.Codigo} apuntaba a una empresa inexistente o dada de baja (Id {persona.IdEmpresa}); se queda sin empresa.");
            persona.IdEmpresa = null;
            cambiado = true;
        }

        return cambiado;
    }

    // Datos con formato incorrecto (por ejemplo, editados a mano): no se tocan, solo se avisa.
    private void AvisarDatosNoValidos()
    {
        foreach (Persona persona in Datos.Personas)
            AvisarSiHayErrores(persona.Codigo, InputValidator.Validar(persona));

        foreach (Empresa empresa in Datos.Empresas)
            AvisarSiHayErrores(empresa.Codigo, InputValidator.Validar(empresa));
    }

    private void AvisarSiHayErrores(string codigo, List<string> errores)
    {
        if (errores.Count > 0)
            avisos.Add($"{codigo} tiene datos no válidos: {string.Join(" ", errores)}");
    }

    private void GuardarReparaciones()
    {
        try
        {
            GuardarCambios();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            avisos.Add($"No se pudieron guardar las reparaciones: {ex.Message}");
        }
    }
}
