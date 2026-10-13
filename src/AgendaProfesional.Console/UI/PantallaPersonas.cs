using AgendaProfesional.Models;
using AgendaProfesional.Services;
using AgendaProfesional.UI.Componentes;
using AgendaProfesional.Utils;

namespace AgendaProfesional.UI;

// Pantallas de personas. Las comunes están en PantallaEntidad; aquí solo va lo propio
// de las personas: sus campos, la baja y el vínculo con una empresa.
public class PantallaPersonas : PantallaEntidad<Persona>
{
    private readonly AgendaService servicio;

    public PantallaPersonas(AgendaService servicio)
    {
        this.servicio = servicio;
    }

    protected override string Singular => "persona";
    protected override string Plural => "personas";
    protected override string PrefijoId => Persona.PrefijoId;
    protected override string TextoBusqueda => "nombre/apellidos";
    protected override string OrdenListado => "apellidos";

    protected override Persona CrearNueva() => new();
    protected override string Describir(Persona persona) => $"{persona.Codigo} - {persona.NombreCompleto}";

    protected override List<Persona> ObtenerTodas() => servicio.ObtenerPersonas();
    protected override List<Persona> BuscarCoincidencias(string criterio) => servicio.BuscarPersonas(criterio);
    protected override Resultado GuardarNueva(Persona persona) => servicio.AltaPersona(persona);
    protected override Resultado GuardarCambios(Persona persona) => servicio.ModificarPersona(persona);

    protected override List<OpcionMenu> CrearOpciones() => new()
    {
        new(1, "Dar de alta una persona", DarDeAlta),
        new(2, "Listar personas", Listar),
        new(3, "Buscar persona", Buscar),
        new(4, "Modificar persona", Modificar),
        new(5, "Dar de baja una persona", DarDeBaja),
    };

    public void DarDeBaja()
    {
        Consola.MostrarTitulo("BAJA DE PERSONA");

        if (Localizar(BuscarCoincidencias) is not Persona persona)
            return;

        Console.WriteLine("Se va a eliminar esta persona:");
        MostrarFicha(persona);

        if (!Consola.LeerConfirmacion($"¿Seguro que quieres eliminar a {Describir(persona)}?"))
        {
            Consola.MostrarAviso("Baja cancelada. No se ha eliminado nada.");
            return;
        }

        if (servicio.EliminarPersona(persona.Id))
            Consola.MostrarExito($"{Describir(persona)} eliminado/a de la agenda.");
        else
            Consola.MostrarError($"{persona.Codigo} ya no existe.");
    }

    protected override List<Campo<Persona>> CrearCampos() => new()
    {
        new("Nombre", true, 12, p => p.Nombre, (p, valor) => p.Nombre = valor,
            InputValidator.NormalizarTexto,
            (valor, _) => InputValidator.ValidarNombre(valor, "Nombre")),

        new("Apellidos", true, 16, p => p.Apellidos, (p, valor) => p.Apellidos = valor,
            InputValidator.NormalizarTexto,
            (valor, _) => InputValidator.ValidarNombre(valor, "Apellidos")),

        new("Teléfono", true, 13, p => p.Telefono, (p, valor) => p.Telefono = valor,
            InputValidator.NormalizarTelefono,
            (valor, _) => InputValidator.ValidarTelefono(valor)),

        new("Correo", true, 24, p => p.Correo, (p, valor) => p.Correo = valor,
            InputValidator.NormalizarCorreo,
            (valor, p) => InputValidator.ValidarCorreo(valor) ?? servicio.ComprobarCorreoLibre(valor, p.Id)),

        // Se escribe el Id de la empresa (E001) y se enseña su nombre.
        new("Empresa", false, 18, CodigoEmpresa, AsignarEmpresa,
            valor => CodigoId.Normalizar(valor, Empresa.PrefijoId),
            (valor, _) => ValidarEmpresa(valor))
        {
            Mostrar = NombreEmpresa,
            Ayuda = MostrarEmpresasDisponibles,
        },

        new("Cargo", false, 14, p => p.Cargo, (p, valor) => p.Cargo = valor,
            InputValidator.NormalizarTexto, Campo<Persona>.SinValidar),
    };

    // ------------------------------------------------------------
    //  Vínculo persona -> empresa
    // ------------------------------------------------------------

    private static string CodigoEmpresa(Persona persona) =>
        persona.IdEmpresa == null ? "" : CodigoId.Formatear(Empresa.PrefijoId, persona.IdEmpresa.Value);

    private static void AsignarEmpresa(Persona persona, string codigo)
    {
        persona.IdEmpresa = CodigoId.IntentarLeer(codigo, Empresa.PrefijoId, out int id) ? id : null;
    }

    private string? ValidarEmpresa(string codigo)
    {
        if (codigo == "")
            return null;

        if (!CodigoId.IntentarLeer(codigo, Empresa.PrefijoId, out int id))
            return "Empresa: escribe un Id como E001, o deja el campo vacío.";

        return servicio.ComprobarEmpresaAsignable(id);
    }

    private string NombreEmpresa(Persona persona)
    {
        if (persona.IdEmpresa == null)
            return "";

        Empresa? empresa = servicio.ObtenerEmpresaActiva(persona.IdEmpresa.Value);
        return empresa?.NombreComercial ?? CodigoEmpresa(persona);
    }

    private void MostrarEmpresasDisponibles()
    {
        List<Empresa> empresas = servicio.ObtenerEmpresasActivas();

        if (empresas.Count == 0)
        {
            Console.WriteLine("  (Todavía no hay empresas: deja el campo vacío.)");
            return;
        }

        Console.WriteLine("  Empresas disponibles:");
        foreach (Empresa empresa in empresas)
            Console.WriteLine($"    {empresa.Codigo} - {empresa.NombreComercial}");
    }
}
