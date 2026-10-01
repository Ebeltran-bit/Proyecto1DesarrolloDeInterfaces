using AgendaConsultora.Modelos;
using AgendaConsultora.Servicios;
using AgendaConsultora.Utilidades;

namespace AgendaConsultora.Interfaz;

// Pantallas de gestión de personas. Solo pide datos y muestra resultados:
// las reglas y el guardado los decide ServicioPersonas.
public class MenuPersonas
{
    private readonly ServicioPersonas servicio;
    private readonly List<CampoPersona> campos;
    private readonly List<Columna<Persona>> columnasTabla;

    public MenuPersonas(ServicioPersonas servicio)
    {
        this.servicio = servicio;
        campos = CrearCampos();
        columnasTabla = CrearColumnasTabla();
    }

    // ------------------------------------------------------------
    //  Opciones del menú
    // ------------------------------------------------------------

    public void DarDeAlta()
    {
        Pantalla.MostrarTitulo("ALTA DE PERSONA");
        Console.WriteLine("Los campos con * son obligatorios.");

        var nueva = new Persona();
        foreach (CampoPersona campo in campos)
        {
            string mensaje = campo.Obligatorio
                ? $"{campo.Etiqueta}*: "
                : $"{campo.Etiqueta} (Enter si no tiene): ";

            campo.Asignar(nueva, PedirValor(campo, nueva, mensaje));
        }

        Console.WriteLine();
        Console.WriteLine("Datos introducidos:");
        MostrarFicha(nueva);

        if (!Pantalla.LeerConfirmacion("¿Guardar esta persona?"))
        {
            Pantalla.MostrarAviso("Alta cancelada. No se ha guardado nada.");
            return;
        }

        Resultado resultado = servicio.DarDeAlta(nueva);

        if (resultado.Correcto)
            Pantalla.MostrarExito($"Alta realizada: {nueva.Codigo} - {nueva.NombreCompleto}.");
        else
            Pantalla.MostrarErrores(resultado.Errores);
    }

    public void Listar()
    {
        Pantalla.MostrarTitulo("LISTADO DE PERSONAS (ordenado por apellidos)");

        List<Persona> personas = servicio.ObtenerTodas();

        if (personas.Count == 0)
        {
            Pantalla.MostrarAviso("La agenda está vacía.");
            return;
        }

        Tabla.Mostrar(personas, columnasTabla);
        Console.WriteLine($"Total: {personas.Count} persona(s).");
    }

    public void Buscar()
    {
        Pantalla.MostrarTitulo("BUSCAR PERSONA");

        string criterio = Pantalla.LeerTexto("Id (ej. P003) o texto del nombre/apellidos: ");
        if (criterio == "")
        {
            Pantalla.MostrarError("Debes escribir algo para buscar.");
            return;
        }

        List<Persona> resultado = servicio.Buscar(criterio);

        if (resultado.Count == 0)
        {
            Pantalla.MostrarAviso($"No se ha encontrado ninguna persona para \"{criterio}\".");
            return;
        }

        Tabla.Mostrar(resultado, columnasTabla);
        Console.WriteLine($"{resultado.Count} coincidencia(s).");
    }

    public void Modificar()
    {
        Pantalla.MostrarTitulo("MODIFICAR PERSONA");

        if (Localizar() is not Persona persona)
            return;

        Console.WriteLine("Persona seleccionada:");
        MostrarFicha(persona);

        // Un submenú generado a partir de los campos: muestra el valor actual de cada uno.
        List<OpcionMenu> opciones = campos
            .Select((campo, indice) => new OpcionMenu(
                indice + 1,
                () => $"{campo.Etiqueta,-10} ({Pantalla.TextoOGuion(campo.Obtener(persona))})",
                () => CambiarCampo(persona, campo)))
            .ToList();

        new Menu($"MODIFICAR {persona.Codigo} - {persona.NombreCompleto}", "Terminar y volver", opciones).Ejecutar();

        Console.WriteLine("Estado final de la persona:");
        MostrarFicha(persona);
    }

    public void DarDeBaja()
    {
        Pantalla.MostrarTitulo("BAJA DE PERSONA");

        if (Localizar() is not Persona persona)
            return;

        Console.WriteLine("Se va a eliminar esta persona:");
        MostrarFicha(persona);

        if (!Pantalla.LeerConfirmacion($"¿Seguro que quieres eliminar a {persona.Codigo} - {persona.NombreCompleto}?"))
        {
            Pantalla.MostrarAviso("Baja cancelada. No se ha eliminado nada.");
            return;
        }

        if (servicio.Eliminar(persona.Id))
            Pantalla.MostrarExito($"{persona.Codigo} - {persona.NombreCompleto} eliminado/a de la agenda.");
        else
            Pantalla.MostrarError($"{persona.Codigo} ya no existe.");
    }

    // ------------------------------------------------------------
    //  Funciones auxiliares
    // ------------------------------------------------------------

    // Pide un Id o texto y devuelve una única persona, o null si no existe.
    // Modificar y dar de baja lo usan para localizar el registro antes de actuar.
    private Persona? Localizar()
    {
        string criterio = Pantalla.LeerTexto("Id o texto del nombre/apellidos (Enter para volver): ");
        if (criterio == "")
        {
            Pantalla.MostrarAviso("Operación cancelada.");
            return null;
        }

        List<Persona> coincidencias = servicio.Buscar(criterio);

        if (coincidencias.Count == 0)
        {
            Pantalla.MostrarError($"No existe ninguna persona que coincida con \"{criterio}\".");
            return null;
        }

        if (coincidencias.Count == 1)
            return coincidencias[0];

        Console.WriteLine($"Hay {coincidencias.Count} coincidencias:");
        Tabla.Mostrar(coincidencias, columnasTabla);

        string idEscrito = Pantalla.LeerTexto("Escribe el Id de la persona: ");
        Persona? elegida = CodigoId.IntentarLeer(idEscrito, Persona.PrefijoId, out int id)
            ? coincidencias.Find(p => p.Id == id)
            : null;

        if (elegida == null)
            Pantalla.MostrarError("Ese Id no está entre las coincidencias.");

        return elegida;
    }

    // Pide el nuevo valor, muestra "antes -> después" y solo guarda si se confirma.
    private void CambiarCampo(Persona persona, CampoPersona campo)
    {
        string mensaje = campo.Obligatorio
            ? $"Nuevo valor de {campo.Etiqueta}: "
            : $"Nuevo valor de {campo.Etiqueta} (Enter para dejarlo vacío): ";

        string valorActual = campo.Obtener(persona);
        string valorNuevo = PedirValor(campo, persona, mensaje);

        if (valorNuevo == valorActual)
        {
            Pantalla.MostrarAviso("El valor es el mismo; no hay nada que cambiar.");
            return;
        }

        Console.WriteLine($"{campo.Etiqueta}: \"{Pantalla.TextoOGuion(valorActual)}\"  ->  \"{Pantalla.TextoOGuion(valorNuevo)}\"");

        if (!Pantalla.LeerConfirmacion("¿Confirmas el cambio?"))
        {
            Pantalla.MostrarAviso("Cambio descartado.");
            return;
        }

        // Se prueba el cambio sobre una copia: si el servicio lo rechaza, la persona no se altera.
        Persona copia = persona.Clonar();
        campo.Asignar(copia, valorNuevo);
        Resultado resultado = servicio.Modificar(copia);

        if (resultado.Correcto)
        {
            campo.Asignar(persona, campo.Obtener(copia));
            Pantalla.MostrarExito($"{campo.Etiqueta} actualizado.");
        }
        else
        {
            Pantalla.MostrarErrores(resultado.Errores);
        }
    }

    private static string PedirValor(CampoPersona campo, Persona persona, string mensaje) =>
        Pantalla.LeerValorValido(mensaje, campo.Normalizar, valor => campo.Validar(valor, persona));

    private void MostrarFicha(Persona persona)
    {
        string codigo = persona.Id == 0 ? "(se asigna al guardar)" : persona.Codigo;
        Console.WriteLine($"  {"Id",-10}: {codigo}");

        foreach (CampoPersona campo in campos)
            Console.WriteLine($"  {campo.Etiqueta,-10}: {Pantalla.TextoOGuion(campo.Obtener(persona))}");
    }

    // ------------------------------------------------------------
    //  Definición de campos y columnas
    // ------------------------------------------------------------

    private List<CampoPersona> CrearCampos()
    {
        // Validación para los campos opcionales: cualquier valor, incluso vacío, es correcto.
        Func<string, Persona, string?> sinValidacion = (_, _) => null;

        return new List<CampoPersona>
        {
            new("Nombre", true, 12, p => p.Nombre, (p, valor) => p.Nombre = valor,
                ValidadorPersona.NormalizarTexto,
                (valor, _) => ValidadorPersona.ValidarNombre(valor, "Nombre")),

            new("Apellidos", true, 16, p => p.Apellidos, (p, valor) => p.Apellidos = valor,
                ValidadorPersona.NormalizarTexto,
                (valor, _) => ValidadorPersona.ValidarNombre(valor, "Apellidos")),

            new("Teléfono", true, 13, p => p.Telefono, (p, valor) => p.Telefono = valor,
                ValidadorPersona.NormalizarTelefono,
                (valor, _) => ValidadorPersona.ValidarTelefono(valor)),

            new("Correo", true, 26, p => p.Correo, (p, valor) => p.Correo = valor,
                ValidadorPersona.NormalizarCorreo,
                (valor, p) => ValidadorPersona.ValidarCorreo(valor) ?? servicio.ComprobarCorreoLibre(valor, p.Id)),

            new("Empresa", false, 14, p => p.Empresa, (p, valor) => p.Empresa = valor,
                ValidadorPersona.NormalizarTexto, sinValidacion),

            new("Cargo", false, 14, p => p.Cargo, (p, valor) => p.Cargo = valor,
                ValidadorPersona.NormalizarTexto, sinValidacion),
        };
    }

    private List<Columna<Persona>> CrearColumnasTabla()
    {
        var columnas = new List<Columna<Persona>> { new("Id", 5, p => p.Codigo) };

        columnas.AddRange(campos.Select(campo =>
            new Columna<Persona>(campo.Etiqueta, campo.AnchoColumna, p => Pantalla.TextoOGuion(campo.Obtener(p)))));

        return columnas;
    }
}
