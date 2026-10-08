using AgendaConsultora.Modelos;
using AgendaConsultora.Servicios;

namespace AgendaConsultora.Datos;

// Datos precargados para la demostración. Se dan de alta a través de los servicios,
// así que pasan las mismas validaciones que los introducidos a mano.
public static class DatosDeEjemplo
{
    public static void Cargar(ServicioEmpresas empresas, ServicioPersonas personas)
    {
        var tech = new Empresa { NombreComercial = "TechSolutions S.L.", Cif = "B12345678", Telefono = "912345678",
                                 Correo = "info@techsolutions.es", Direccion = "Calle Mayor 10, Madrid" };
        var innova = new Empresa { NombreComercial = "Innova Consulting", Cif = "A87654321", Telefono = "934567890",
                                   Correo = "contacto@innovaconsulting.com", Direccion = "Av. Diagonal 200, Barcelona" };
        // Sin personas vinculadas: sirve para probar la baja y la recuperación.
        var logistica = new Empresa { NombreComercial = "Logística Sur S.L.", Cif = "B11223344", Telefono = "952112233",
                                      Correo = "admin@logisticasur.es", Direccion = "Calle Larios 5, Málaga" };

        // El alta asigna el Id a cada empresa, que después se usa para vincular a las personas.
        empresas.DarDeAlta(tech);
        empresas.DarDeAlta(innova);
        empresas.DarDeAlta(logistica);

        var contactos = new List<Persona>
        {
            new() { Nombre = "Laura", Apellidos = "Martínez Ruiz", Telefono = "612345678",
                    Correo = "laura.martinez@techsolutions.es", IdEmpresa = tech.Id, Cargo = "Jefa de proyecto" },
            new() { Nombre = "Carlos", Apellidos = "García López", Telefono = "698765432",
                    Correo = "cgarcia@innovaconsulting.com", IdEmpresa = innova.Id, Cargo = "Consultor senior" },
            new() { Nombre = "Ana", Apellidos = "Fernández Gil", Telefono = "634112233",
                    Correo = "ana.fernandez@techsolutions.es", IdEmpresa = tech.Id, Cargo = "Desarrolladora" },
            new() { Nombre = "Javier", Apellidos = "Sánchez Moreno", Telefono = "655443322",
                    Correo = "jsanchez@gmail.com" },
        };

        foreach (Persona persona in contactos)
            personas.DarDeAlta(persona);
    }
}
