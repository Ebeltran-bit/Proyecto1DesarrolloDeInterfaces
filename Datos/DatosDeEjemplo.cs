using AgendaConsultora.Modelos;
using AgendaConsultora.Servicios;

namespace AgendaConsultora.Datos;

// Personas precargadas para la demostración. Se dan de alta a través del servicio,
// así que pasan las mismas validaciones que las introducidas a mano.
public static class DatosDeEjemplo
{
    public static void Cargar(ServicioPersonas servicio)
    {
        var personas = new List<Persona>
        {
            new() { Nombre = "Laura", Apellidos = "Martínez Ruiz", Telefono = "612345678",
                    Correo = "laura.martinez@techsolutions.es", Empresa = "TechSolutions S.L.", Cargo = "Jefa de proyecto" },
            new() { Nombre = "Carlos", Apellidos = "García López", Telefono = "698765432",
                    Correo = "cgarcia@innovaconsulting.com", Empresa = "Innova Consulting", Cargo = "Consultor senior" },
            new() { Nombre = "Ana", Apellidos = "Fernández Gil", Telefono = "634112233",
                    Correo = "ana.fernandez@techsolutions.es", Empresa = "TechSolutions S.L.", Cargo = "Desarrolladora" },
            new() { Nombre = "Javier", Apellidos = "Sánchez Moreno", Telefono = "655443322",
                    Correo = "jsanchez@gmail.com" },
        };

        foreach (Persona persona in personas)
            servicio.DarDeAlta(persona);
    }
}
