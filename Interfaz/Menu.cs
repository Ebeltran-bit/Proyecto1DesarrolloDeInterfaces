namespace AgendaConsultora.Interfaz;

// Una opción de menú. El texto es una función para poder mostrar valores que cambian
// (por ejemplo, el valor actual de cada campo en el menú de modificar).
public record OpcionMenu(int Numero, Func<string> Texto, Action Accion)
{
    public OpcionMenu(int numero, string texto, Action accion)
        : this(numero, () => texto, accion)
    {
    }
}

// Menú numerado reutilizable: se repite hasta elegir 0 y avisa si la opción no existe.
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
            int eleccion = Pantalla.LeerEntero("Elige una opción: ");
            Console.WriteLine();

            if (eleccion == 0)
                return;

            OpcionMenu? opcion = opciones.Find(o => o.Numero == eleccion);

            if (opcion == null)
                Pantalla.MostrarError($"Esa opción no existe. Elige un número del 0 al {opciones.Count}.");
            else
                opcion.Accion();
        }
    }

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
