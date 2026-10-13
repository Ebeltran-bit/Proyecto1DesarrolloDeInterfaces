using AgendaProfesional.Models;

namespace AgendaProfesional.Infrastructure;

// Operaciones comunes a personas y empresas. Trabaja sobre una lista del contexto y
// guarda los cambios después de cada alta, modificación o borrado.
// Entrega y guarda copias: modificar un objeto recibido no cambia nada hasta llamar a Actualizar.
public abstract class RepositorioBase<T> : IRepositorio<T> where T : class, IEntidad<T>
{
    protected ContextoAgenda Contexto { get; }

    protected RepositorioBase(ContextoAgenda contexto)
    {
        Contexto = contexto;
    }

    // Cada repositorio concreto indica su lista y su contador de Id.
    protected abstract List<T> Elementos { get; }

    protected abstract int SiguienteId();

    public List<T> ObtenerTodas() => Copiar(Elementos);

    public T? ObtenerPorId(int id) => Primero(e => e.Id == id);

    public void Agregar(T elemento)
    {
        elemento.Id = SiguienteId();
        Elementos.Add(elemento.Clonar());
        Contexto.GuardarCambios();
    }

    public void Actualizar(T elemento)
    {
        int posicion = Elementos.FindIndex(e => e.Id == elemento.Id);
        if (posicion == -1)
            throw new InvalidOperationException($"No existe ningún registro con Id {elemento.Id}.");

        Elementos[posicion] = elemento.Clonar();
        Contexto.GuardarCambios();
    }

    public bool Eliminar(int id)
    {
        bool eliminado = Elementos.RemoveAll(e => e.Id == id) > 0;

        if (eliminado)
            Contexto.GuardarCambios();

        return eliminado;
    }

    // Para las consultas propias de cada repositorio.
    protected List<T> Filtrar(Func<T, bool> condicion) => Copiar(Elementos.Where(condicion));

    protected T? Primero(Func<T, bool> condicion) => Elementos.FirstOrDefault(condicion)?.Clonar();

    private static List<T> Copiar(IEnumerable<T> origen) => origen.Select(e => e.Clonar()).ToList();
}
