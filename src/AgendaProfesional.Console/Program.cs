using AgendaProfesional.Infrastructure;
using AgendaProfesional.Services;
using AgendaProfesional.UI;

// Punto de entrada: crea las piezas, las conecta y arranca la consola.
// Toda la lógica está en AgendaService y toda la interacción en ConsoleUI.

ContextoAgenda contexto = new(CrearAlmacen(args));

var servicio = new AgendaService(new RepositorioPersonas(contexto), new RepositorioEmpresas(contexto));

if (servicio.EstaVacia())
    DatosDeEjemplo.Cargar(servicio);

new ConsoleUI(servicio, contexto.Avisos).Ejecutar();


// Por defecto los datos se guardan en datos/agenda.json (en la carpeta desde la que se ejecuta).
// Con el argumento --sin-archivo solo se guardan en memoria: útil para pruebas.
static IAlmacenAgenda CrearAlmacen(string[] argumentos) =>
    argumentos.Contains("--sin-archivo")
        ? new AlmacenMemoria()
        : new AlmacenJson(Path.Combine(Environment.CurrentDirectory, "datos", "agenda.json"));
