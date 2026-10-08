using AgendaConsultora.Modelos;
using AgendaConsultora.Servicios;

namespace AgendaConsultora.Interfaz;

// Operaciones sobre la relación 1:N: una persona tiene cero o una empresa;
// una empresa tiene cero, una o muchas personas.
// Reutiliza los menús de personas y empresas para elegir registros y mostrarlos.
public class MenuRelaciones
{
    private readonly ServicioPersonas servicioPersonas;
    private readonly ServicioEmpresas servicioEmpresas;
    private readonly MenuPersonas menuPersonas;
    private readonly MenuEmpresas menuEmpresas;

    public MenuRelaciones(ServicioPersonas servicioPersonas, ServicioEmpresas servicioEmpresas,
                          MenuPersonas menuPersonas, MenuEmpresas menuEmpresas)
    {
        this.servicioPersonas = servicioPersonas;
        this.servicioEmpresas = servicioEmpresas;
        this.menuPersonas = menuPersonas;
        this.menuEmpresas = menuEmpresas;
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
        Pantalla.MostrarTitulo("ASIGNAR O CAMBIAR EMPRESA");

        if (ElegirPersona() is not Persona persona)
            return;

        Console.WriteLine($"Empresa actual: {NombreEmpresa(persona)}");

        Console.WriteLine("Elige la empresa:");
        if (menuEmpresas.Seleccionar() is not Empresa empresa)
            return;

        if (persona.IdEmpresa == empresa.Id)
        {
            Pantalla.MostrarAviso($"{persona.Codigo} ya pertenece a {empresa.Codigo} - {empresa.NombreComercial}.");
            return;
        }

        string pregunta = persona.IdEmpresa == null
            ? $"¿Asignar {persona.Codigo} - {persona.NombreCompleto} a {empresa.Codigo} - {empresa.NombreComercial}?"
            : $"¿Cambiar {persona.Codigo} - {persona.NombreCompleto} de {NombreEmpresa(persona)} a {empresa.Codigo} - {empresa.NombreComercial}?";

        if (!Pantalla.LeerConfirmacion(pregunta))
        {
            Pantalla.MostrarAviso("Operación cancelada. No se ha cambiado nada.");
            return;
        }

        MostrarResultado(servicioPersonas.AsignarEmpresa(persona.Id, empresa.Id),
                         $"{persona.Codigo} pertenece ahora a {empresa.Codigo} - {empresa.NombreComercial}.");
    }

    public void Desvincular()
    {
        Pantalla.MostrarTitulo("DESVINCULAR PERSONA");

        if (ElegirPersona() is not Persona persona)
            return;

        if (persona.IdEmpresa == null)
        {
            Pantalla.MostrarAviso($"{persona.Codigo} - {persona.NombreCompleto} no tiene empresa asignada.");
            return;
        }

        if (!Pantalla.LeerConfirmacion($"¿Desvincular a {persona.Codigo} - {persona.NombreCompleto} de {NombreEmpresa(persona)}? (Sus datos se conservan)"))
        {
            Pantalla.MostrarAviso("Operación cancelada. No se ha cambiado nada.");
            return;
        }

        MostrarResultado(servicioPersonas.Desvincular(persona.Id),
                         $"{persona.Codigo} - {persona.NombreCompleto} ya no tiene empresa asignada.");
    }

    public void VerEmpresaDePersona()
    {
        Pantalla.MostrarTitulo("EMPRESA DE UNA PERSONA");

        if (ElegirPersona() is not Persona persona)
            return;

        Empresa? empresa = persona.IdEmpresa == null ? null : servicioEmpresas.ObtenerActiva(persona.IdEmpresa.Value);

        if (empresa == null)
        {
            Pantalla.MostrarAviso($"{persona.Codigo} - {persona.NombreCompleto} no tiene empresa asignada.");
            return;
        }

        Console.WriteLine($"{persona.Codigo} - {persona.NombreCompleto} pertenece a:");
        menuEmpresas.MostrarFicha(empresa);
    }

    public void VerPlantilla()
    {
        Pantalla.MostrarTitulo("PERSONAS DE UNA EMPRESA");

        Console.WriteLine("Elige la empresa:");
        if (menuEmpresas.Seleccionar() is not Empresa empresa)
            return;

        MostrarPersonasDe($"{empresa.Codigo} - {empresa.NombreComercial}", servicioPersonas.ObtenerPorEmpresa(empresa.Id));
    }

    // Vista final del proyecto: qué personas pertenecen a cada empresa.
    public void VerResumen()
    {
        Pantalla.MostrarTitulo("PERSONAS POR EMPRESA");

        foreach (Empresa empresa in servicioEmpresas.ObtenerActivas())
            MostrarPersonasDe($"{empresa.Codigo} - {empresa.NombreComercial}", servicioPersonas.ObtenerPorEmpresa(empresa.Id));

        MostrarPersonasDe("Sin empresa asignada", servicioPersonas.ObtenerSinEmpresa());
    }

    // ------------------------------------------------------------
    //  Funciones auxiliares
    // ------------------------------------------------------------

    private Persona? ElegirPersona()
    {
        Console.WriteLine("Elige la persona:");
        return menuPersonas.Seleccionar();
    }

    private void MostrarPersonasDe(string titulo, List<Persona> personas)
    {
        Console.WriteLine();
        Console.WriteLine($"{titulo}: {personas.Count} persona(s)");

        if (personas.Count > 0)
            menuPersonas.MostrarTabla(personas);
    }

    private string NombreEmpresa(Persona persona)
    {
        if (persona.IdEmpresa == null)
            return "(sin empresa)";

        Empresa? empresa = servicioEmpresas.ObtenerActiva(persona.IdEmpresa.Value);
        return empresa == null ? "(sin empresa)" : $"{empresa.Codigo} - {empresa.NombreComercial}";
    }

    private static void MostrarResultado(Resultado resultado, string mensajeExito)
    {
        if (resultado.Correcto)
            Pantalla.MostrarExito(mensajeExito);
        else
            Pantalla.MostrarErrores(resultado.Errores);
    }
}
