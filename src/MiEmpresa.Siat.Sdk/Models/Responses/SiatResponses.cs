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

/// <summary>Elemento de un catálogo paramétrico del SIN (código + descripción).</summary>
public sealed record ParametroSiat
{
    public required int Codigo { get; init; }
    public required string Descripcion { get; init; }
}

/// <summary>Resultado de sincronizar una paramétrica (catálogo) del SIN.</summary>
public sealed record SincronizacionParametricaResponse
{
    public required bool Transaccion { get; init; }
    public required IReadOnlyList<ParametroSiat> Parametros { get; init; }
    public required IReadOnlyList<SiatMensaje> Mensajes { get; init; }
    public bool Exitoso => Transaccion && Mensajes.All(m => !m.EsError);
}

/// <summary>Actividad económica (CAEB) del contribuyente.</summary>
public sealed record ActividadEconomicaSiat
{
    public required string CodigoCaeb { get; init; }
    public required string Descripcion { get; init; }
    public required string TipoActividad { get; init; }
}

/// <summary>Producto o servicio homologado del SIN, asociado a una actividad.</summary>
public sealed record ProductoServicioSiat
{
    public required string CodigoActividad { get; init; }
    public required int CodigoProducto { get; init; }
    public required string DescripcionProducto { get; init; }
}

/// <summary>Leyenda de factura (Ley 453) asociada a una actividad.</summary>
public sealed record LeyendaFacturaSiat
{
    public required string CodigoActividad { get; init; }
    public required string DescripcionLeyenda { get; init; }
}

/// <summary>Documento sector habilitado para una actividad del contribuyente.</summary>
public sealed record ActividadDocumentoSectorSiat
{
    public required string CodigoActividad { get; init; }
    public required int CodigoDocumentoSector { get; init; }
    public required string TipoDocumentoSector { get; init; }
}

/// <summary>Resultado de un catálogo con forma propia (no `{codigo, descripcion}`).</summary>
public sealed record CatalogoResponse<T>
{
    public required bool Transaccion { get; init; }
    public required IReadOnlyList<T> Items { get; init; }
    public required IReadOnlyList<SiatMensaje> Mensajes { get; init; }
    public bool Exitoso => Transaccion && Mensajes.All(m => !m.EsError);
}

/// <summary>Fecha y hora oficial del SIN (usada para `fechaEmision`).</summary>
public sealed record FechaHoraResponse
{
    public required bool Transaccion { get; init; }
    public required DateTime FechaHora { get; init; }
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