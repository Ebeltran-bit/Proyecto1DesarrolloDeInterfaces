namespace AgendaProfesional.Infrastructure;

// No guarda nada: los datos solo viven mientras el programa está abierto.
// Útil para pruebas o demostraciones que no deben dejar rastro (dotnet run -- --sin-archivo).
public class AlmacenMemoria : IAlmacenAgenda
{
    public DatosAgenda? Cargar() => null;

    public void Guardar(DatosAgenda datos)
    {
    }
}
