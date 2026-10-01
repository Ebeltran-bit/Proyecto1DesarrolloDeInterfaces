namespace AgendaConsultora.Interfaz;

// Lectura y escritura por consola. Ninguna entrada incorrecta termina el programa:
// las funciones de lectura repiten la pregunta hasta obtener un valor válido.
public static class Pantalla
{
    // ---------- Lectura ----------

    public static string LeerTexto(string mensaje)
    {
        Console.Write(mensaje);
        string? linea = Console.ReadLine();

        // null solo llega si se cierra la entrada (Ctrl+Z / Ctrl+D): se sale de forma ordenada.
        if (linea == null)
        {
            Console.WriteLine();
            Environment.Exit(0);
        }

        return linea.Trim();
    }

    public static int LeerEntero(string mensaje)
    {
        while (true)
        {
            if (int.TryParse(LeerTexto(mensaje), out int numero))
                return numero;

            MostrarError("Debes escribir un número.");
        }
    }

    public static bool LeerConfirmacion(string pregunta)
    {
        while (true)
        {
            string respuesta = LeerTexto($"{pregunta} (S/N): ").ToUpperInvariant();

            if (respuesta is "S" or "SI" or "SÍ")
                return true;
            if (respuesta is "N" or "NO")
                return false;

            MostrarError("Responde S (sí) o N (no).");
        }
    }

    // Pide un valor, lo normaliza y lo valida; si no es válido muestra el error y lo vuelve a pedir.
    public static string LeerValorValido(string mensaje, Func<string, string> normalizar, Func<string, string?> validar)
    {
        while (true)
        {
            string valor = normalizar(LeerTexto(mensaje));
            string? error = validar(valor);

            if (error == null)
                return valor;

            MostrarError(error);
        }
    }

    // ---------- Escritura ----------

    public static void MostrarTitulo(string titulo) => Console.WriteLine($"--- {titulo} ---");

    public static void MostrarExito(string mensaje) => EscribirEnColor($"[OK] {mensaje}", ConsoleColor.Green);

    public static void MostrarError(string mensaje) => EscribirEnColor($"[ERROR] {mensaje}", ConsoleColor.Red);

    public static void MostrarAviso(string mensaje) => EscribirEnColor($"[AVISO] {mensaje}", ConsoleColor.Yellow);

    public static void MostrarErrores(IEnumerable<string> errores)
    {
        foreach (string error in errores)
            MostrarError(error);
    }

    public static string TextoOGuion(string texto) => texto == "" ? "-" : texto;

    private static void EscribirEnColor(string texto, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(texto);
        Console.ResetColor();
    }
}
