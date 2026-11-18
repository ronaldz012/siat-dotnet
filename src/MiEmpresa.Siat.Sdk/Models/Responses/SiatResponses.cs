namespace MiEmpresa.Siat.Sdk.Models.Responses;

public sealed record TokenResponse
{
    public required string Token { get; init; }
    public required DateTime Expiracion { get; init; }
    public required long Nit { get; init; }
    public required string CodigoSistema { get; init; }
}

public sealed record CuisResponse
{
    public required string Codigo { get; init; }
    public required DateTime FechaVigencia { get; init; }
    public required string CodigoControl { get; init; }
    public required bool Transaccion { get; init; }
    public required IReadOnlyList<SiatMensaje> Mensajes { get; init; }
    public bool Exitoso => Transaccion && Mensajes.All(m => !m.EsError);
}

public sealed record CufdResponse
{
    public required string Codigo { get; init; }
    public required string CodigoControl { get; init; }
    public required string Direccion { get; init; }
    public required DateTime FechaVigencia { get; init; }
    public required bool Transaccion { get; init; }
    public required IReadOnlyList<SiatMensaje> Mensajes { get; init; }
    public bool Exitoso => Transaccion && Mensajes.All(m => !m.EsError);
}

public sealed record EmisionResponse
{
    public required bool Transaccion { get; init; }
    public required string? CodigoRecepcion { get; init; }
    public required string? Cuf { get; init; }
    public required IReadOnlyList<SiatMensaje> Mensajes { get; init; }
    public bool Exitoso => Transaccion && Mensajes.All(m => !m.EsError);
}

public sealed record EmisionPaqueteResponse
{
    public required bool Transaccion { get; init; }
    public required string? CodigoRecepcion { get; init; }
    public required IReadOnlyList<SiatMensaje> Mensajes { get; init; }
    public bool Exitoso => Transaccion && Mensajes.All(m => !m.EsError);
}

public sealed record AnulacionResponse
{
    public required bool Transaccion { get; init; }
    public required string? CodigoRecepcion { get; init; }
    public required IReadOnlyList<SiatMensaje> Mensajes { get; init; }
    public bool Exitoso => Transaccion && Mensajes.All(m => !m.EsError);
}

public sealed record ReversionResponse
{
    public required bool Transaccion { get; init; }
    public required string? CodigoRecepcion { get; init; }
    public required IReadOnlyList<SiatMensaje> Mensajes { get; init; }
    public bool Exitoso => Transaccion && Mensajes.All(m => !m.EsError);
}

public sealed record RegistroEventoResponse
{
    public required bool Transaccion { get; init; }
    public required int? CodigoEvento { get; init; }
    public required IReadOnlyList<SiatMensaje> Mensajes { get; init; }
    public bool Exitoso => Transaccion && Mensajes.All(m => !m.EsError);
}

public sealed record SincronizacionResponse
{
    public required bool Transaccion { get; init; }
    public required IReadOnlyList<SiatMensaje> Mensajes { get; init; }
    public required object? Datos { get; init; }
    public bool Exitoso => Transaccion && Mensajes.All(m => !m.EsError);
}

public sealed record HomologacionResponse
{
    public required bool Transaccion { get; init; }
    public required IReadOnlyList<SiatMensaje> Mensajes { get; init; }
    public required object? Datos { get; init; }
    public bool Exitoso => Transaccion && Mensajes.All(m => !m.EsError);
}

public sealed record SiatMensaje
{
    public required int Codigo { get; init; }
    public required string Descripcion { get; init; }
    public bool EsError => Codigo >= 1000; // Códigos >= 1000 son errores según SIAT (900-999 son warnings/informativos)
    public bool EsWarning => Codigo >= 900 && Codigo < 1000;
}