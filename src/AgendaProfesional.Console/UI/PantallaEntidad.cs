using AgendaProfesional.Models;
using AgendaProfesional.Services;
using AgendaProfesional.UI.Componentes;
using AgendaProfesional.Utils;

namespace AgendaProfesional.UI;

// Pantallas comunes a personas y empresas: alta, listado, búsqueda y modificación.
// Cada pantalla concreta solo indica sus textos, sus campos y qué operación del servicio usar.
public abstract class PantallaEntidad<T> where T : class, IEntidad<T>
{
    private List<Campo<T>>? campos;
    private List<Columna<T>>? columnas;

    // ------------------------------------------------------------
    //  Lo que define cada pantalla concreta
    // ------------------------------------------------------------

    protected abstract string Singular { get; }          // "persona"
    protected abstract string Plural { get; }            // "personas"
    protected abstract string PrefijoId { get; }         // "P"
    protected abstract string TextoBusqueda { get; }     // "nombre/apellidos"
    protected abstract string OrdenListado { get; }      // "apellidos"

    protected abstract List<Campo<T>> CrearCampos();
    protected abstract List<OpcionMenu> CrearOpciones();
    protected abstract T CrearNueva();
    protected abstract string Describir(T entidad);      // "P001 - Laura Martínez Ruiz"

    protected abstract List<T> ObtenerTodas();
    protected abstract List<T> BuscarCoincidencias(string criterio);
    protected abstract Resultado GuardarNueva(T entidad);
    protected abstract Resultado GuardarCambios(T entidad);

    private List<Campo<T>> Campos => campos ??= CrearCampos();
    private List<Columna<T>> Columnas => columnas ??= CrearColumnas();
    private int AnchoEtiquetas => Campos.Max(c => c.Etiqueta.Length);

    // ------------------------------------------------------------
    //  Opciones comunes
    // ------------------------------------------------------------

    public void Ejecutar() =>
        new Menu($"GESTIÓN DE {Plural.ToUpper()}", "Volver al menú principal", CrearOpciones()).Ejecutar();

    public void DarDeAlta()
    {
        Consola.MostrarTitulo($"ALTA DE {Singular.ToUpper()}");
        Console.WriteLine("Los campos con * son obligatorios.");

        T nueva = CrearNueva();
        foreach (Campo<T> campo in Campos)
        {
            string mensaje = campo.Obligatorio
                ? $"{campo.Etiqueta}*: "
                : $"{campo.Etiqueta} (Enter si no tiene): ";

            campo.Asignar(nueva, PedirValor(campo, nueva, mensaje));
        }

        Console.WriteLine();
        Console.WriteLine("Datos introducidos:");
        MostrarFicha(nueva);

        if (!Consola.LeerConfirmacion($"¿Guardar esta {Singular}?"))
        {
            Consola.MostrarAviso("Alta cancelada. No se ha guardado nada.");
            return;
        }

        Resultado resultado = GuardarNueva(nueva);

        if (resultado.Correcto)
            Consola.MostrarExito($"Alta realizada: {Describir(nueva)}.");
        else
            Consola.MostrarErrores(resultado.Errores);
    }

    public void Listar()
    {
        Consola.MostrarTitulo($"LISTADO DE {Plural.ToUpper()} (ordenado por {OrdenListado})");

        List<T> todas = ObtenerTodas();

        if (todas.Count == 0)
        {
            Consola.MostrarAviso($"No hay {Plural} registradas.");
            return;
        }

        MostrarTabla(todas);
        Console.WriteLine($"Total: {todas.Count} {Singular}(s).");
    }

    public void Buscar()
    {
        Consola.MostrarTitulo($"BUSCAR {Singular.ToUpper()}");

        string criterio = Consola.LeerTexto($"Id (ej. {CodigoId.Formatear(PrefijoId, 3)}) o texto del {TextoBusqueda}: ");
        if (criterio == "")
        {
            Consola.MostrarError("Debes escribir algo para buscar.");
            return;
        }

        List<T> resultado = BuscarCoincidencias(criterio);

        if (resultado.Count == 0)
        {
            Consola.MostrarAviso($"No se ha encontrado ninguna {Singular} para \"{criterio}\".");
            return;
        }

        MostrarTabla(resultado);
        Console.WriteLine($"{resultado.Count} coincidencia(s).");
    }

    public void Modificar()
    {
        Consola.MostrarTitulo($"MODIFICAR {Singular.ToUpper()}");

        if (Localizar(BuscarCoincidencias) is not T entidad)
            return;

        Console.WriteLine($"{Mayuscula(Singular)} seleccionada:");
        MostrarFicha(entidad);

        // Un submenú generado a partir de los campos: muestra el valor actual de cada uno.
        List<OpcionMenu> opciones = Campos
            .Select((campo, indice) => new OpcionMenu(
                indice + 1,
                () => $"{campo.Etiqueta.PadRight(AnchoEtiquetas)} ({campo.TextoVisible(entidad)})",
                () => CambiarCampo(entidad, campo)))
            .ToList();

        new Menu($"MODIFICAR {Describir(entidad)}", "Terminar y volver", opciones).Ejecutar();

        Console.WriteLine($"Estado final de la {Singular}:");
        MostrarFicha(entidad);
    }

    // ------------------------------------------------------------
    //  Funciones para las pantallas concretas
    // ------------------------------------------------------------

    // Pide un Id o texto y devuelve un único registro, o null si no existe.
    // Modificar, dar de baja y recuperar lo usan para localizar el registro antes de actuar.
    protected T? Localizar(Func<string, List<T>> buscar)
    {
        string criterio = Consola.LeerTexto($"Id o texto del {TextoBusqueda} (Enter para volver): ");
        if (criterio == "")
        {
            Consola.MostrarAviso("Operación cancelada.");
            return null;
        }

        List<T> coincidencias = buscar(criterio);

        if (coincidencias.Count == 0)
        {
            Consola.MostrarError($"No existe ninguna {Singular} que coincida con \"{criterio}\".");
            return null;
        }

        if (coincidencias.Count == 1)
            return coincidencias[0];

        Console.WriteLine($"Hay {coincidencias.Count} coincidencias:");
        MostrarTabla(coincidencias);

        string idEscrito = Consola.LeerTexto($"Escribe el Id de la {Singular}: ");
        T? elegida = CodigoId.IntentarLeer(idEscrito, PrefijoId, out int id)
            ? coincidencias.Find(e => e.Id == id)
            : null;

        if (elegida == null)
            Consola.MostrarError("Ese Id no está entre las coincidencias.");

        return elegida;
    }

    // Para otras pantallas (relaciones): elegir un registro por Id o texto.
    public T? Seleccionar() => Localizar(BuscarCoincidencias);

    public void MostrarTabla(IEnumerable<T> elementos) => Tabla.Mostrar(elementos, Columnas);

    public void MostrarFicha(T entidad)
    {
        string codigo = entidad.Id == 0 ? "(se asigna al guardar)" : entidad.Codigo;
        Console.WriteLine($"  {"Id".PadRight(AnchoEtiquetas)} : {codigo}");

        foreach (Campo<T> campo in Campos)
            Console.WriteLine($"  {campo.Etiqueta.PadRight(AnchoEtiquetas)} : {campo.TextoVisible(entidad)}");
    }

    // ------------------------------------------------------------
    //  Funciones internas
    // ------------------------------------------------------------

    // Pide el nuevo valor, muestra "antes -> después" y solo guarda si se confirma.
    private void CambiarCampo(T entidad, Campo<T> campo)
    {
        string mensaje = campo.Obligatorio
            ? $"Nuevo valor de {campo.Etiqueta}: "
            : $"Nuevo valor de {campo.Etiqueta} (Enter para dejarlo vacío): ";

        string valorNuevo = PedirValor(campo, entidad, mensaje);

        if (valorNuevo == campo.Obtener(entidad))
        {
            Consola.MostrarAviso("El valor es el mismo; no hay nada que cambiar.");
            return;
        }

        // El cambio se aplica sobre una copia: si no se confirma o el servicio lo rechaza,
        // el registro original no se altera.
        T copia = entidad.Clonar();
        campo.Asignar(copia, valorNuevo);

        Console.WriteLine($"{campo.Etiqueta}: \"{campo.TextoVisible(entidad)}\"  ->  \"{campo.TextoVisible(copia)}\"");

        if (!Consola.LeerConfirmacion("¿Confirmas el cambio?"))
        {
            Consola.MostrarAviso("Cambio descartado.");
            return;
        }

        Resultado resultado = GuardarCambios(copia);

        if (resultado.Correcto)
        {
            campo.Asignar(entidad, campo.Obtener(copia));
            Consola.MostrarExito($"{campo.Etiqueta} actualizado.");
        }
        else
        {
            Consola.MostrarErrores(resultado.Errores);
        }
    }

    private static string PedirValor(Campo<T> campo, T entidad, string mensaje)
    {
        campo.Ayuda?.Invoke();
        return Consola.LeerValorValido(mensaje, campo.Normalizar, valor => campo.Validar(valor, entidad));
    }

    private List<Columna<T>> CrearColumnas()
    {
        var resultado = new List<Columna<T>> { new("Id", 5, e => e.Codigo) };

        resultado.AddRange(Campos.Select(campo =>
            new Columna<T>(campo.Etiqueta, campo.AnchoColumna, campo.TextoVisible)));

        return resultado;
    }

    private static string Mayuscula(string texto) => char.ToUpper(texto[0]) + texto[1..];
}
