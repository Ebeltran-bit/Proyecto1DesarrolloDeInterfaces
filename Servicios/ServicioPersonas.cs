using AgendaConsultora.Datos;
using AgendaConsultora.Modelos;
using AgendaConsultora.Utilidades;

namespace AgendaConsultora.Servicios;

// Lógica de negocio de las personas: validar antes de guardar, correos únicos, búsquedas
// y orden del listado. No escribe nada en pantalla, así que la podrá usar tal cual
// cualquier interfaz futura (escritorio, web...).
public class ServicioPersonas
{
    private readonly IRepositorioPersonas repositorio;

    public ServicioPersonas(IRepositorioPersonas repositorio)
    {
        this.repositorio = repositorio;
    }

    public List<Persona> ObtenerTodas() => OrdenarPorApellidos(repositorio.ObtenerTodas());

    // Si el criterio tiene forma de código (P3, p003) busca por Id;
    // si no, busca el texto dentro de nombre y apellidos.
    public List<Persona> Buscar(string criterio)
    {
        if (CodigoId.IntentarLeer(criterio, Persona.PrefijoId, out int id))
        {
            Persona? persona = repositorio.ObtenerPorId(id);
            return persona == null ? new List<Persona>() : new List<Persona> { persona };
        }

        return OrdenarPorApellidos(repositorio.BuscarPorNombre(criterio.Trim()));
    }

    // idPropio es el de la persona que se está editando (0 en un alta),
    // para que su propio correo no cuente como repetido.
    public string? ComprobarCorreoLibre(string correo, int idPropio)
    {
        Persona? duenio = repositorio.ObtenerPorCorreo(correo);

        if (duenio == null || duenio.Id == idPropio)
            return null;

        return $"Correo: ya pertenece a {duenio.Codigo} - {duenio.NombreCompleto}.";
    }

    public Resultado DarDeAlta(Persona persona)
    {
        Resultado resultado = Validar(persona);

        if (resultado.Correcto)
            repositorio.Agregar(persona);

        return resultado;
    }

    public Resultado Modificar(Persona persona)
    {
        if (repositorio.ObtenerPorId(persona.Id) == null)
            return Resultado.Error($"No existe la persona {persona.Codigo}.");

        Resultado resultado = Validar(persona);

        if (resultado.Correcto)
            repositorio.Actualizar(persona);

        return resultado;
    }

    public bool Eliminar(int id) => repositorio.Eliminar(id);

    private Resultado Validar(Persona persona)
    {
        ValidadorPersona.Normalizar(persona);

        List<string> errores = ValidadorPersona.Validar(persona);

        string? errorCorreo = ComprobarCorreoLibre(persona.Correo, persona.Id);
        if (errorCorreo != null)
            errores.Add(errorCorreo);

        return Resultado.Desde(errores);
    }

    private static List<Persona> OrdenarPorApellidos(IEnumerable<Persona> personas) =>
        personas.OrderBy(p => p.Apellidos, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(p => p.Nombre, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
}
