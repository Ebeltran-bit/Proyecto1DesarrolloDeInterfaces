namespace AgendaConsultora.Interfaz;

// Columna de una tabla: título, ancho en caracteres y cómo obtener el valor de cada fila.
public record Columna<T>(string Titulo, int Ancho, Func<T, string> Valor);

// Dibuja cualquier lista como tabla alineada. En la Fase 2 servirá también para empresas.
public static class Tabla
{
    private const string SeparadorColumnas = " | ";

    public static void Mostrar<T>(IEnumerable<T> filas, IReadOnlyList<Columna<T>> columnas)
    {
        string cabecera = UnirCeldas(columnas.Select(c => Ajustar(c.Titulo, c.Ancho)));
        string separador = new('-', cabecera.Length);

        Console.WriteLine(separador);
        Console.WriteLine(cabecera);
        Console.WriteLine(separador);

        foreach (T fila in filas)
            Console.WriteLine(UnirCeldas(columnas.Select(c => Ajustar(c.Valor(fila), c.Ancho))));

        Console.WriteLine(separador);
    }

    private static string UnirCeldas(IEnumerable<string> celdas) => string.Join(SeparadorColumnas, celdas);

    // Rellena con espacios hasta el ancho de la columna, o recorta con "…" si no cabe.
    private static string Ajustar(string texto, int ancho) =>
        texto.Length <= ancho ? texto.PadRight(ancho) : texto[..(ancho - 1)] + "…";
}
