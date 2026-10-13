namespace AgendaProfesional.Infrastructure;

// Dónde se guardan los datos. Hay dos implementaciones: AlmacenJson (archivo) y
// AlmacenMemoria (sin archivo, para pruebas). Una base de datos sería una tercera.
public interface IAlmacenAgenda
{
    // Devuelve null si todavía no hay datos guardados.
    DatosAgenda? Cargar();

    void Guardar(DatosAgenda datos);
}

// El almacén existe pero su contenido no se puede leer.
public class DatosDanadosException : Exception
{
    public DatosDanadosException(string mensaje) : base(mensaje)
    {
    }
}
