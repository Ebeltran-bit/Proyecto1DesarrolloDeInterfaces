using AgendaConsultora.Modelos;

namespace AgendaConsultora.Datos;

// Guarda los elementos en una lista mientras el programa está abierto.
// Entrega y guarda copias para comportarse como una base de datos: un cambio solo
// queda guardado al llamar a Actualizar.
public abstract class RepositorioMemoria<T> : IRepositorio<T> where T : class, IEntidad<T>
{
    private readonly List<T> elementos = new();

    // Funciona como un autoincremental: solo avanza, un Id borrado no se reutiliza.
    private int ultimoId;

    public List<T> ObtenerTodas() => Copiar(elementos);

    public T? ObtenerPorId(int id) => Primero(e => e.Id == id);

    public void Agregar(T elemento)
    {
        ultimoId++;
        elemento.Id = ultimoId;
        elementos.Add(elemento.Clonar());
    }

    public void Actualizar(T elemento)
    {
        int posicion = elementos.FindIndex(e => e.Id == elemento.Id);
        if (posicion == -1)
            throw new InvalidOperationException($"No existe ningún registro con Id {elemento.Id}.");

        elementos[posicion] = elemento.Clonar();
    }

    public bool Eliminar(int id) => elementos.RemoveAll(e => e.Id == id) > 0;

    // Para las consultas propias de cada repositorio.
    protected List<T> Filtrar(Func<T, bool> condicion) => Copiar(elementos.Where(condicion));

    protected T? Primero(Func<T, bool> condicion) => elementos.FirstOrDefault(condicion)?.Clonar();

    private static List<T> Copiar(IEnumerable<T> origen) => origen.Select(e => e.Clonar()).ToList();
}
