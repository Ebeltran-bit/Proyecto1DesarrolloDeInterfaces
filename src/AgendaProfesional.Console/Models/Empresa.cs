using AgendaProfesional.Utils;

namespace AgendaProfesional.Models;

// Se guarda como 0 / 1, igual que en el archivo de datos.
public enum EstadoEmpresa
{
    Baja = 0,
    Activa = 1,
}

// Datos de una empresa con la que colabora la consultora.
public class Empresa : IEntidad<Empresa>
{
    public const string PrefijoId = "E";

    public int Id { get; set; }

    public string NombreComercial { get; set; } = "";
    public string Cif { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Direccion { get; set; } = "";

    // Borrado lógico: dar de baja pone el estado a 0 y la empresa deja de mostrarse,
    // pero el registro se conserva y se puede recuperar volviendo a ponerlo a 1.
    public EstadoEmpresa Estado { get; set; } = EstadoEmpresa.Activa;

    public bool EstaActiva => Estado == EstadoEmpresa.Activa;

    public string Codigo => CodigoId.Formatear(PrefijoId, Id);

    public Empresa Clonar() => (Empresa)MemberwiseClone();
}
