using AgendaProfesional.Models;
using AgendaProfesional.Services;
using AgendaProfesional.UI.Componentes;

namespace AgendaProfesional.UI;

// Operaciones sobre la relación 1:N: una persona tiene cero o una empresa;
// una empresa tiene cero, una o muchas personas.
// Reutiliza las pantallas de personas y empresas para elegir registros y mostrarlos.
public class PantallaRelaciones
{
    private readonly AgendaService servicio;
    private readonly PantallaPersonas pantallaPersonas;
    private readonly PantallaEmpresas pantallaEmpresas;

    public PantallaRelaciones(AgendaService servicio, PantallaPersonas pantallaPersonas, PantallaEmpresas pantallaEmpresas)
    {
        this.servicio = servicio;
        this.pantallaPersonas = pantallaPersonas;
        this.pantallaEmpresas = pantallaEmpresas;
    }

    public void Ejecutar() =>
        new Menu("RELACIONES PERSONA - EMPRESA", "Volver al menú principal", new List<OpcionMenu>
        {
            new(1, "Asignar o cambiar la empresa de una persona", AsignarEmpresa),
            new(2, "Desvincular una persona de su empresa", Desvincular),
            new(3, "Ver la empresa de una persona", VerEmpresaDePersona),
            new(4, "Ver las personas de una empresa", VerPlantilla),
            new(5, "Resumen: personas por empresa", VerResumen),
        }).Ejecutar();

    public void AsignarEmpresa()
    {
        Consola.MostrarTitulo("ASIGNAR O CAMBIAR EMPRESA");

        if (ElegirPersona() is not Persona persona)
            return;

        Console.WriteLine($"Empresa actual: {NombreEmpresa(persona)}");

        Console.WriteLine("Elige la empresa:");
        if (pantallaEmpresas.Seleccionar() is not Empresa empresa)
            return;

        if (persona.IdEmpresa == empresa.Id)
        {
            Consola.MostrarAviso($"{persona.Codigo} ya pertenece a {empresa.Codigo} - {empresa.NombreComercial}.");
            return;
        }

        string pregunta = persona.IdEmpresa == null
            ? $"¿Asignar {persona.Codigo} - {persona.NombreCompleto} a {empresa.Codigo} - {empresa.NombreComercial}?"
            : $"¿Cambiar {persona.Codigo} - {persona.NombreCompleto} de {NombreEmpresa(persona)} a {empresa.Codigo} - {empresa.NombreComercial}?";

        if (!Consola.LeerConfirmacion(pregunta))
        {
            Consola.MostrarAviso("Operación cancelada. No se ha cambiado nada.");
            return;
        }

        MostrarResultado(servicio.AsignarEmpresa(persona.Id, empresa.Id),
                         $"{persona.Codigo} pertenece ahora a {empresa.Codigo} - {empresa.NombreComercial}.");
    }

    public void Desvincular()
    {
        Consola.MostrarTitulo("DESVINCULAR PERSONA");

        if (ElegirPersona() is not Persona persona)
            return;

        if (persona.IdEmpresa == null)
        {
            Consola.MostrarAviso($"{persona.Codigo} - {persona.NombreCompleto} no tiene empresa asignada.");
            return;
        }

        if (!Consola.LeerConfirmacion($"¿Desvincular a {persona.Codigo} - {persona.NombreCompleto} de {NombreEmpresa(persona)}? (Sus datos se conservan)"))
        {
            Consola.MostrarAviso("Operación cancelada. No se ha cambiado nada.");
            return;
        }

        MostrarResultado(servicio.DesvincularPersona(persona.Id),
                         $"{persona.Codigo} - {persona.NombreCompleto} ya no tiene empresa asignada.");
    }

    public void VerEmpresaDePersona()
    {
        Consola.MostrarTitulo("EMPRESA DE UNA PERSONA");

        if (ElegirPersona() is not Persona persona)
            return;

        Empresa? empresa = persona.IdEmpresa == null ? null : servicio.ObtenerEmpresaActiva(persona.IdEmpresa.Value);

        if (empresa == null)
        {
            Consola.MostrarAviso($"{persona.Codigo} - {persona.NombreCompleto} no tiene empresa asignada.");
            return;
        }

        Console.WriteLine($"{persona.Codigo} - {persona.NombreCompleto} pertenece a:");
        pantallaEmpresas.MostrarFicha(empresa);
    }

    public void VerPlantilla()
    {
        Consola.MostrarTitulo("PERSONAS DE UNA EMPRESA");

        Console.WriteLine("Elige la empresa:");
        if (pantallaEmpresas.Seleccionar() is not Empresa empresa)
            return;

        MostrarPersonasDe($"{empresa.Codigo} - {empresa.NombreComercial}", servicio.ObtenerPersonasDeEmpresa(empresa.Id));
    }

    // Vista final del proyecto: qué personas pertenecen a cada empresa.
    public void VerResumen()
    {
        Consola.MostrarTitulo("PERSONAS POR EMPRESA");

        foreach (Empresa empresa in servicio.ObtenerEmpresasActivas())
            MostrarPersonasDe($"{empresa.Codigo} - {empresa.NombreComercial}", servicio.ObtenerPersonasDeEmpresa(empresa.Id));

        MostrarPersonasDe("Sin empresa asignada", servicio.ObtenerPersonasSinEmpresa());
    }

    // ------------------------------------------------------------
    //  Funciones auxiliares
    // ------------------------------------------------------------

    private Persona? ElegirPersona()
    {
        Console.WriteLine("Elige la persona:");
        return pantallaPersonas.Seleccionar();
    }

    private void MostrarPersonasDe(string titulo, List<Persona> personas)
    {
        Console.WriteLine();
        Console.WriteLine($"{titulo}: {personas.Count} persona(s)");

        if (personas.Count > 0)
            pantallaPersonas.MostrarTabla(personas);
    }

    private string NombreEmpresa(Persona persona)
    {
        if (persona.IdEmpresa == null)
            return "(sin empresa)";

        Empresa? empresa = servicio.ObtenerEmpresaActiva(persona.IdEmpresa.Value);
        return empresa == null ? "(sin empresa)" : $"{empresa.Codigo} - {empresa.NombreComercial}";
    }

    private static void MostrarResultado(Resultado resultado, string mensajeExito)
    {
        if (resultado.Correcto)
            Consola.MostrarExito(mensajeExito);
        else
            Consola.MostrarErrores(resultado.Errores);
    }
}
