using AgendaConsultora.Modelos;
using AgendaConsultora.Servicios;

namespace AgendaConsultora.Interfaz;

// Menú de empresas. Las pantallas comunes están en MenuEntidad; aquí solo va lo propio
// de las empresas: sus campos, la baja lógica y la recuperación.
public class MenuEmpresas : MenuEntidad<Empresa>
{
    private readonly ServicioEmpresas servicio;

    public MenuEmpresas(ServicioEmpresas servicio)
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
    protected override List<Empresa> ObtenerTodas() => servicio.ObtenerActivas();
    protected override List<Empresa> BuscarCoincidencias(string criterio) => servicio.Buscar(criterio);
    protected override Resultado GuardarNueva(Empresa empresa) => servicio.DarDeAlta(empresa);
    protected override Resultado GuardarCambios(Empresa empresa) => servicio.Modificar(empresa);

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
        Pantalla.MostrarTitulo("BAJA DE EMPRESA");

        if (Localizar(BuscarCoincidencias) is not Empresa empresa)
            return;

        Console.WriteLine("Empresa seleccionada:");
        MostrarFicha(empresa);

        List<Persona> vinculadas = servicio.ObtenerPersonas(empresa.Id);
        if (vinculadas.Count > 0)
        {
            Pantalla.MostrarError($"No se puede dar de baja: tiene {vinculadas.Count} persona(s) vinculada(s).");
            foreach (Persona persona in vinculadas)
                Console.WriteLine($"    {persona.Codigo} - {persona.NombreCompleto}");
            Console.WriteLine("Cámbialas de empresa o elimínalas antes desde el menú de personas.");
            return;
        }

        if (!Pantalla.LeerConfirmacion($"¿Seguro que quieres dar de baja a {Describir(empresa)}?"))
        {
            Pantalla.MostrarAviso("Baja cancelada. La empresa sigue activa.");
            return;
        }

        Resultado resultado = servicio.DarDeBaja(empresa.Id);

        if (resultado.Correcto)
            Pantalla.MostrarExito($"{Describir(empresa)} dada de baja. Ya no aparece en listados ni búsquedas, pero se puede recuperar.");
        else
            Pantalla.MostrarErrores(resultado.Errores);
    }

    // Vuelve a poner la empresa en Estado 1 para que se muestre de nuevo.
    public void Recuperar()
    {
        Pantalla.MostrarTitulo("RECUPERAR EMPRESA");

        List<Empresa> deBaja = servicio.ObtenerDeBaja();
        if (deBaja.Count == 0)
        {
            Pantalla.MostrarAviso("No hay empresas dadas de baja.");
            return;
        }

        Console.WriteLine("Empresas dadas de baja:");
        MostrarTabla(deBaja);

        if (Localizar(servicio.BuscarDeBaja) is not Empresa empresa)
            return;

        if (!Pantalla.LeerConfirmacion($"¿Recuperar {Describir(empresa)}?"))
        {
            Pantalla.MostrarAviso("Operación cancelada. La empresa sigue dada de baja.");
            return;
        }

        Resultado resultado = servicio.Recuperar(empresa.Id);

        if (resultado.Correcto)
            Pantalla.MostrarExito($"{Describir(empresa)} recuperada. Vuelve a aparecer en listados y búsquedas.");
        else
            Pantalla.MostrarErrores(resultado.Errores);
    }

    protected override List<Campo<Empresa>> CrearCampos() => new()
    {
        new("Nombre comercial", true, 22, e => e.NombreComercial, (e, valor) => e.NombreComercial = valor,
            Validador.NormalizarTexto,
            (valor, _) => Validador.ValidarObligatorio(valor, "Nombre comercial")),

        new("CIF", true, 9, e => e.Cif, (e, valor) => e.Cif = valor,
            ValidadorEmpresa.NormalizarCif,
            (valor, e) => ValidadorEmpresa.ValidarCif(valor) ?? servicio.ComprobarCifLibre(valor, e.Id)),

        new("Teléfono", true, 13, e => e.Telefono, (e, valor) => e.Telefono = valor,
            Validador.NormalizarTelefono,
            (valor, _) => Validador.ValidarTelefono(valor)),

        new("Correo", true, 26, e => e.Correo, (e, valor) => e.Correo = valor,
            Validador.NormalizarCorreo,
            (valor, _) => Validador.ValidarCorreo(valor)),

        new("Dirección", true, 28, e => e.Direccion, (e, valor) => e.Direccion = valor,
            Validador.NormalizarTexto,
            (valor, _) => Validador.ValidarObligatorio(valor, "Dirección")),
    };
}
