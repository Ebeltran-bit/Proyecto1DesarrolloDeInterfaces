using System.Globalization;

namespace AgendaConsultora.Utilidades;

public static class Texto
{
    // Busca sin distinguir mayúsculas ni tildes: "garcia" encuentra "García".
    public static bool Contiene(string texto, string buscado) =>
        CultureInfo.InvariantCulture.CompareInfo.IndexOf(
            texto, buscado, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
}
