using AgendaConsultora.Modelos;
using AgendaConsultora.Utilidades;

namespace AgendaConsultora.Datos;

// Guarda las personas en una lista mientras el programa está abierto.
// Entrega y guarda copias para comportarse como una base de datos: un cambio solo
// queda guardado al llamar a Actualizar.
public class RepositorioPersonasMemoria : IRepositorioPersonas
{
    private readonly List<Persona> personas = new();

    // Funciona como un autoincremental: solo avanza, un Id borrado no se reutiliza.
    private int ultimoId;

    public List<Persona> ObtenerTodas() => Copiar(personas);

    public Persona? ObtenerPorId(int id) =>
        personas.Find(p => p.Id == id)?.Clonar();

    public Persona? ObtenerPorCorreo(string correo) =>
        personas.Find(p => p.Correo.Equals(correo, StringComparison.OrdinalIgnoreCase))?.Clonar();

    public List<Persona> BuscarPorNombre(string texto) =>
        Copiar(personas.Where(p => Texto.Contiene(p.NombreCompleto, texto)));

    public void Agregar(Persona persona)
    {
        ultimoId++;
        persona.Id = ultimoId;
        personas.Add(persona.Clonar());
    }

    public void Actualizar(Persona persona)
    {
        int posicion = personas.FindIndex(p => p.Id == persona.Id);
        if (posicion == -1)
            throw new InvalidOperationException($"No existe ninguna persona con Id {persona.Id}.");

        personas[posicion] = persona.Clonar();
    }

    public bool Eliminar(int id) => personas.RemoveAll(p => p.Id == id) > 0;

    private static List<Persona> Copiar(IEnumerable<Persona> origen) =>
        origen.Select(p => p.Clonar()).ToList();
}
