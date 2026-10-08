using System.Text;
using AgendaConsultora.Datos;
using AgendaConsultora.Interfaz;
using AgendaConsultora.Servicios;

// Punto de entrada: aquí solo se crean las piezas y se conectan entre sí.

Console.OutputEncoding = Encoding.UTF8;

// Almacenamiento elegido. Para pasar a base de datos basta con cambiar estas dos líneas
// por los repositorios de BD: servicios y menús no se enteran del cambio.
IRepositorioPersonas repositorioPersonas = new RepositorioPersonasMemoria();
IRepositorioEmpresas repositorioEmpresas = new RepositorioEmpresasMemoria();

var servicioPersonas = new ServicioPersonas(repositorioPersonas, repositorioEmpresas);
var servicioEmpresas = new ServicioEmpresas(repositorioEmpresas, repositorioPersonas);
DatosDeEjemplo.Cargar(servicioEmpresas, servicioPersonas);

var menuPersonas = new MenuPersonas(servicioPersonas, servicioEmpresas);
var menuEmpresas = new MenuEmpresas(servicioEmpresas);
var menuRelaciones = new MenuRelaciones(servicioPersonas, servicioEmpresas, menuPersonas, menuEmpresas);

// Cada submenú vuelve aquí al elegir 0; la aplicación solo termina al salir de este menú.
var menuPrincipal = new Menu("AGENDA DE LA CONSULTORA", "Salir", new List<OpcionMenu>
{
    new(1, "Gestión de personas", menuPersonas.Ejecutar),
    new(2, "Gestión de empresas", menuEmpresas.Ejecutar),
    new(3, "Relaciones persona - empresa", menuRelaciones.Ejecutar),
});

menuPrincipal.Ejecutar();
Pantalla.MostrarExito("Hasta pronto.");
