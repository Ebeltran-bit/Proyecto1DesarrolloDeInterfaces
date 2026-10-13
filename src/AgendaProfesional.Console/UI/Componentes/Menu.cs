namespace AgendaProfesional.UI.Componentes;

// Una opción de menú. El texto es una función para poder mostrar valores que cambian
// (por ejemplo, el valor actual de cada campo en el menú de modificar).
public record OpcionMenu(int Numero, Func<string> Texto, Action Accion)
{
    public OpcionMenu(int numero, string texto, Action accion)
        : this(numero, () => texto, accion)
    {
    }
}

// Menú numerado reutilizable: se repite hasta elegir 0. La validación de la opción la hace Consola.LeerOpcion.
public class Menu
{
    private readonly string titulo;
    private readonly string textoSalida;
    private readonly List<OpcionMenu> opciones;

    public Menu(string titulo, string textoSalida, List<OpcionMenu> opciones)
    {
        this.titulo = titulo;
        this.textoSalida = textoSalida;
        this.opciones = opciones;
    }

    public void Ejecutar()
    {
        while (true)
        {
            MostrarOpciones();
            int eleccion = Consola.LeerOpcion("Elige una opción: ", NumerosValidos());
            Console.WriteLine();

            if (eleccion == 0)
                return;

            EjecutarOpcion(opciones.First(o => o.Numero == eleccion));
        }
    }

    // Si no se pueden guardar los datos (archivo bloqueado, sin permisos...) se avisa
    // y se vuelve al menú: el programa no termina por un fallo al guardar.
    private static void EjecutarOpcion(OpcionMenu opcion)
    {
        try
        {
            opcion.Accion();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Consola.MostrarError($"No se pudieron guardar los datos: {ex.Message}");
        }
    }

    // El 0 (salir o volver) siempre es válido.
    private List<int> NumerosValidos() => opciones.Select(o => o.Numero).Prepend(0).ToList();

    private void MostrarOpciones()
    {
        string linea = new('=', 42);

        Console.WriteLine();
        Console.WriteLine(linea);
        Console.WriteLine($"  {titulo}");
        Console.WriteLine(linea);

        foreach (OpcionMenu opcion in opciones)
            Console.WriteLine($"  {opcion.Numero}. {opcion.Texto()}");

        Console.WriteLine($"  0. {textoSalida}");
        Console.WriteLine(new string('-', 42));
    }
}
