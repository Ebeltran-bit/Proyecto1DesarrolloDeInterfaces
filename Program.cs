using System.Text;
using AgendaConsultora.Datos;
using AgendaConsultora.Interfaz;
using AgendaConsultora.Servicios;

// Punto de entrada: aquí solo se crean las piezas y se conectan entre sí.

Console.OutputEncoding = Encoding.UTF8;

// Almacenamiento elegido. En la Fase 2 basta con cambiar esta línea por el repositorio
// de base de datos: servicios y menús no se enteran del cambio.
IRepositorioPersonas repositorioPersonas = new RepositorioPersonasMemoria();

var servicioPersonas = new ServicioPersonas(repositorioPersonas);
DatosDeEjemplo.Cargar(servicioPersonas);

var menuPersonas = new MenuPersonas(servicioPersonas);

var menuPrincipal = new Menu("AGENDA DE LA CONSULTORA", "Salir", new List<OpcionMenu>
{
    new(1, "Dar de alta una persona", menuPersonas.DarDeAlta),
    new(2, "Listar personas", menuPersonas.Listar),
    new(3, "Buscar persona", menuPersonas.Buscar),
    new(4, "Modificar persona", menuPersonas.Modificar),
    new(5, "Dar de baja una persona", menuPersonas.DarDeBaja),
});

menuPrincipal.Ejecutar();
Pantalla.MostrarExito("Hasta pronto.");
