using AgendaProfesional.Models;
using AgendaProfesional.Services;
using AgendaProfesional.UI.Componentes;
using AgendaProfesional.Utils;

namespace AgendaProfesional.UI;

// Pantallas de empresas. Las comunes están en PantallaEntidad; aquí solo va lo propio
// de las empresas: sus campos, la baja lógica y la recuperación.
public class PantallaEmpresas : PantallaEntidad<Empresa>
{
    private readonly AgendaService servicio;

    public PantallaEmpresas(AgendaService servicio)
    {
        this.servicio = servicio;
    }

    protected override string Singular => "empresa";
    protected override string Plural => "empresas";
    protected override string PrefijoId => Empresa.PrefijoId;
    protected override string TextoBusqueda => "nombre comercial o CIF";
    protected override string OrdenListado => "nombre comercial";

    protected override Empresa CrearNueva() => new();
    protected override string Describir(Empresa empresa) => $"{empresa.Codigo} - {empresa.NombreComercial}";

    // Solo se enseñan las empresas activas (Estado 1).
    protected override List<Empresa> ObtenerTodas() => servicio.ObtenerEmpresasActivas();
    protected override List<Empresa> BuscarCoincidencias(string criterio) => servicio.BuscarEmpresas(criterio);
    protected override Resultado GuardarNueva(Empresa empresa) => servicio.AltaEmpresa(empresa);
    protected override Resultado GuardarCambios(Empresa empresa) => servicio.ModificarEmpresa(empresa);

    protected override List<OpcionMenu> CrearOpciones() => new()
    {
        new(1, "Dar de alta una empresa", DarDeAlta),
        new(2, "Listar empresas", Listar),
        new(3, "Buscar empresa", Buscar),
        new(4, "Modificar empresa", Modificar),
        new(5, "Dar de baja una empresa", DarDeBaja),
        new(6, "Recuperar una empresa dada de baja", Recuperar),
    };

    // Baja lógica: la empresa pasa a Estado 0 y deja de mostrarse, pero no se borra.
    // Solo se permite si no tiene personas vinculadas.
    public void DarDeBaja()
    {
        Consola.MostrarTitulo("BAJA DE EMPRESA");

        if (Localizar(BuscarCoincidencias) is not Empresa empresa)
            return;

        Console.WriteLine("Empresa seleccionada:");
        MostrarFicha(empresa);

        List<Persona> vinculadas = servicio.ObtenerPersonasDeEmpresa(empresa.Id);
        if (vinculadas.Count > 0)
        {
            Consola.MostrarError($"No se puede dar de baja {Describir(empresa)}: tiene {vinculadas.Count} persona(s) vinculada(s).");
            foreach (Persona persona in vinculadas)
                Console.WriteLine($"    {persona.Codigo} - {persona.NombreCompleto}");
            Consola.MostrarAviso("Política: una empresa con personas no se puede borrar. Desvincúlalas o cámbialas " +
                                  "de empresa antes (menú principal -> 3. Relaciones persona - empresa).");
            return;
        }

        if (!Consola.LeerConfirmacion($"¿Seguro que quieres dar de baja a {Describir(empresa)}?"))
        {
            Consola.MostrarAviso("Baja cancelada. La empresa sigue activa.");
            return;
        }

        Resultado resultado = servicio.DarDeBajaEmpresa(empresa.Id);

        if (resultado.Correcto)
            Consola.MostrarExito($"{Describir(empresa)} dada de baja. Ya no aparece en listados ni búsquedas, pero se puede recuperar.");
        else
            Consola.MostrarErrores(resultado.Errores);
    }

    // Vuelve a poner la empresa en Estado 1 para que se muestre de nuevo.
    public void Recuperar()
    {
        Consola.MostrarTitulo("RECUPERAR EMPRESA");

        List<Empresa> deBaja = servicio.ObtenerEmpresasDeBaja();
        if (deBaja.Count == 0)
        {
            Consola.MostrarAviso("No hay empresas dadas de baja.");
            return;
        }

        Console.WriteLine("Empresas dadas de baja:");
        MostrarTabla(deBaja);

        if (Localizar(servicio.BuscarEmpresasDeBaja) is not Empresa empresa)
            return;

        if (!Consola.LeerConfirmacion($"¿Recuperar {Describir(empresa)}?"))
        {
            Consola.MostrarAviso("Operación cancelada. La empresa sigue dada de baja.");
            return;
        }

        Resultado resultado = servicio.RecuperarEmpresa(empresa.Id);

        if (resultado.Correcto)
            Consola.MostrarExito($"{Describir(empresa)} recuperada. Vuelve a aparecer en listados y búsquedas.");
        else
            Consola.MostrarErrores(resultado.Errores);
    }

    protected override List<Campo<Empresa>> CrearCampos() => new()
    {
        new("Nombre comercial", true, 22, e => e.NombreComercial, (e, valor) => e.NombreComercial = valor,
            InputValidator.NormalizarTexto,
            (valor, _) => InputValidator.ValidarObligatorio(valor, "Nombre comercial")),

        new("CIF", true, 9, e => e.Cif, (e, valor) => e.Cif = valor,
            InputValidator.NormalizarCif,
            (valor, e) => InputValidator.ValidarCif(valor) ?? servicio.ComprobarCifLibre(valor, e.Id)),

        new("Teléfono", true, 13, e => e.Telefono, (e, valor) => e.Telefono = valor,
            InputValidator.NormalizarTelefono,
            (valor, _) => InputValidator.ValidarTelefono(valor)),

        new("Correo", true, 26, e => e.Correo, (e, valor) => e.Correo = valor,
            InputValidator.NormalizarCorreo,
            (valor, _) => InputValidator.ValidarCorreo(valor)),

        new("Dirección", true, 28, e => e.Direccion, (e, valor) => e.Direccion = valor,
            InputValidator.NormalizarTexto,
            (valor, _) => InputValidator.ValidarObligatorio(valor, "Dirección")),
    };
}
