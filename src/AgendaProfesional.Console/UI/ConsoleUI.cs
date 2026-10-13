using System.Text;
using AgendaProfesional.Services;
using AgendaProfesional.UI.Componentes;

namespace AgendaProfesional.UI;

// Interfaz de consola: muestra los menús, pide los datos y presenta los resultados.
// No toca las colecciones: todo lo que lee o cambia lo pide a AgendaService.
// Cada bloque de pantallas está en su propia clase (personas, empresas, relaciones).
public class ConsoleUI
{
    private readonly PantallaPersonas pantallaPersonas;
    private readonly PantallaEmpresas pantallaEmpresas;
    private readonly PantallaRelaciones pantallaRelaciones;
    private readonly IReadOnlyList<string> avisosDeInicio;

    public ConsoleUI(AgendaService servicio, IReadOnlyList<string> avisosDeInicio)
    {
        pantallaPersonas = new PantallaPersonas(servicio);
        pantallaEmpresas = new PantallaEmpresas(servicio);
        pantallaRelaciones = new PantallaRelaciones(servicio, pantallaPersonas, pantallaEmpresas);
        this.avisosDeInicio = avisosDeInicio;
    }

    public void Ejecutar()
    {
        Console.OutputEncoding = Encoding.UTF8;

        MostrarAvisosDeInicio();
        CrearMenuPrincipal().Ejecutar();

        Consola.MostrarExito("Hasta pronto.");
    }

    // Cada submenú vuelve aquí al elegir 0; la aplicación solo termina al salir de este menú.
    private Menu CrearMenuPrincipal() => new("AGENDA DE LA CONSULTORA", "Salir", new List<OpcionMenu>
    {
        new(1, "Gestión de personas", pantallaPersonas.Ejecutar),
        new(2, "Gestión de empresas", pantallaEmpresas.Ejecutar),
        new(3, "Relaciones persona - empresa", pantallaRelaciones.Ejecutar),
    });

    // Problemas detectados al cargar el archivo de datos (y cómo se resolvieron).
    private void MostrarAvisosDeInicio()
    {
        foreach (string aviso in avisosDeInicio)
            Consola.MostrarAviso(aviso);
    }
}
