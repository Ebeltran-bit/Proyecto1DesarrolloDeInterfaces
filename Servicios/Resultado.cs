namespace AgendaConsultora.Servicios;

// Respuesta de una operación que puede fallar por datos no válidos.
// Sirve igual para la consola que para una futura interfaz gráfica o web.
public class Resultado
{
    public IReadOnlyList<string> Errores { get; }

    public bool Correcto => Errores.Count == 0;

    private Resultado(IReadOnlyList<string> errores)
    {
        Errores = errores;
    }

    // Sin errores = operación correcta.
    public static Resultado Desde(IEnumerable<string> errores) => new(errores.ToList());

    public static Resultado Error(string mensaje) => new(new List<string> { mensaje });
}
