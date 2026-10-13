using System.Text.Encodings.Web;
using System.Text.Json;

namespace AgendaProfesional.Infrastructure;

// Guarda toda la agenda en un único archivo JSON. Un solo archivo para personas y
// empresas evita que queden desincronizadas (una persona apuntando a una empresa
// que está en otro archivo que no se llegó a guardar).
public class AlmacenJson : IAlmacenAgenda
{
    private static readonly JsonSerializerOptions Opciones = new()
    {
        WriteIndented = true,
        IgnoreReadOnlyProperties = true,                     // no guarda Codigo, NombreCompleto...
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // tildes legibles en el archivo
    };

    private readonly string ruta;

    public AlmacenJson(string ruta)
    {
        this.ruta = ruta;
    }

    public DatosAgenda? Cargar()
    {
        if (!File.Exists(ruta))
            return null;

        try
        {
            return JsonSerializer.Deserialize<DatosAgenda>(File.ReadAllText(ruta), Opciones)
                   ?? throw new JsonException("el archivo está vacío");
        }
        catch (JsonException ex)
        {
            // Se aparta el archivo dañado en vez de borrarlo, para poder revisarlo a mano.
            string copia = $"{ruta}.danado-{DateTime.Now:yyyyMMdd-HHmmss}";
            File.Move(ruta, copia);
            throw new DatosDanadosException(
                $"El archivo de datos no se pudo leer ({ex.Message}). Se ha apartado como " +
                $"{Path.GetFileName(copia)} y se empieza con datos nuevos.");
        }
    }

    // Escritura segura: primero se escribe un archivo temporal y después sustituye al
    // original. Si el programa se corta a mitad, el archivo bueno sigue intacto.
    public void Guardar(DatosAgenda datos)
    {
        string? carpeta = Path.GetDirectoryName(Path.GetFullPath(ruta));
        if (carpeta != null)
            Directory.CreateDirectory(carpeta);

        string temporal = ruta + ".tmp";
        File.WriteAllText(temporal, JsonSerializer.Serialize(datos, Opciones));
        File.Move(temporal, ruta, overwrite: true);
    }
}
