namespace MiEmpresa.Siat.Sdk.Models;

/// <summary>
/// El SIN envía las fechas con offset (ej. 2027-11-17T15:58:05-04:00). XmlSerializer las
/// entrega como Kind=Local y Npgsql exige Utc para columnas timestamptz. Normalizamos en
/// el borde del SDK para que todos los consumidores reciban instantes en UTC.
/// </summary>
internal static class SiatDate
{
    public static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
    };
}
