namespace MiEmpresa.Siat.Sdk.Models.Requests;

using System.Security.Cryptography.X509Certificates;

public sealed record TokenRequest
{
    public required string Usuario { get; init; }
    public required string Password { get; init; }
}

public sealed record CuisRequest
{
    public required long Nit { get; init; }
    public required string CodigoSistema { get; init; }
    public required Ambiente Ambiente { get; init; }
    public required Modalidad Modalidad { get; init; }
    public int CodigoSucursal { get; init; } = 0;
    public int CodigoPuntoVenta { get; init; } = 0;
}

public sealed record CufdRequest
{
    public required long Nit { get; init; }
    public required string CodigoSistema { get; init; }
    public required Ambiente Ambiente { get; init; }
    public required Modalidad Modalidad { get; init; }
    public required string Cuis { get; init; }
    public int CodigoSucursal { get; init; } = 0;
    public int CodigoPuntoVenta { get; init; } = 0;
}

public sealed record FacturaDetalleRequest
{
    public required int ActividadEconomica { get; init; }
    public required int CodigoProductoSin { get; init; }
    public required int CodigoUnidadMedida { get; init; }
    public required decimal Cantidad { get; init; }
    public required string CodigoProducto { get; init; }
    public required string Descripcion { get; init; }
    public required decimal PrecioUnitario { get; init; }
    public decimal? MontoDescuento { get; init; }
    public required decimal SubTotal { get; init; }
    public string? NumeroImei { get; init; }
    public string? NumeroSerie { get; init; }
}

public sealed record FacturaCabeceraRequest
{
    public required long NitEmisor { get; init; }
    public required string RazonSocialEmisor { get; init; }
    public required string Municipio { get; init; }
    public required string Telefono { get; init; }
    public required int NumeroFactura { get; init; }
    public required string Cufd { get; init; }
    public required string Cuis { get; init; }
    public required string CodigoSistema { get; init; }
    public required Ambiente Ambiente { get; init; }
    public required Modalidad Modalidad { get; init; }
    public required int CodigoSucursal { get; init; }
    public required int CodigoPuntoVenta { get; init; }
    public required DateTime FechaEmision { get; init; }
    public required string NombreRazonSocialCliente { get; init; }
    public required TipoDocumentoIdentidad CodigoTipoDocumentoIdentidad { get; init; }
    public required string NumeroDocumentoCliente { get; init; }
    public required string CodigoCliente { get; init; }
    public required CodigoMetodoPago CodigoMetodoPago { get; init; }
    public required decimal MontoTotal { get; init; }
    public required CodigoMoneda CodigoMoneda { get; init; }
    public required decimal TipoCambio { get; init; }
    public required decimal MontoTotalMoneda { get; init; }
    public required decimal MontoTotalSujetoIva { get; init; }
    public required int CodigoDocumentoSector { get; init; }
    public required TipoFacturaDocumento TipoFacturaDocumento { get; init; }
    public required TipoEmision CodigoEmision { get; init; }
    public decimal? MontoGiftCard { get; init; }
    public decimal? DescuentoAdicional { get; init; }
    public string? CodigoExcepcion { get; init; }
    public string? Ccafc { get; init; }
    public string? Leyenda { get; init; }
    public string? Usuario { get; init; }
    public string? Direccion { get; init; }
    public string? Complemento { get; init; }
}

public sealed record FacturaRequest
{
    public required FacturaCabeceraRequest Cabecera { get; init; }
    public required IReadOnlyList<FacturaDetalleRequest> Detalles { get; init; }
    public required X509Certificate2? CertificadoFirma { get; init; }
}

public sealed record PaqueteFacturasRequest
{
    public required IReadOnlyList<FacturaRequest> Facturas { get; init; }
    public required string Cuis { get; init; }
    public required string Cufd { get; init; }
    public required int CodigoEvento { get; init; }
    public string? Ccafc { get; init; }
    public required X509Certificate2? CertificadoFirma { get; init; }
}

public sealed record AnulacionRequest
{
    public required long Nit { get; init; }
    public required string CodigoSistema { get; init; }
    public required Ambiente Ambiente { get; init; }
    public required Modalidad Modalidad { get; init; }
    public required string Cuis { get; init; }
    public required string Cufd { get; init; }
    public required int CodigoSucursal { get; init; }
    public required int CodigoPuntoVenta { get; init; }
    public required string Cuf { get; init; }
    public required int CodigoDocumentoSector { get; init; }
    public required TipoFacturaDocumento TipoFacturaDocumento { get; init; }
    public required TipoEmision CodigoEmision { get; init; }
    public required CodigoMotivoAnulacion CodigoMotivo { get; init; }
}

public sealed record ReversionRequest
{
    public required long Nit { get; init; }
    public required string CodigoSistema { get; init; }
    public required Ambiente Ambiente { get; init; }
    public required Modalidad Modalidad { get; init; }
    public required string Cuis { get; init; }
    public required string Cufd { get; init; }
    public required int CodigoSucursal { get; init; }
    public required int CodigoPuntoVenta { get; init; }
    public required string Cuf { get; init; }
    public required int CodigoDocumentoSector { get; init; }
    public required TipoFacturaDocumento TipoFacturaDocumento { get; init; }
    public required TipoEmision CodigoEmision { get; init; }
}

public sealed record EventoSignificativoRequest
{
    public required long Nit { get; init; }
    public required string CodigoSistema { get; init; }
    public required Ambiente Ambiente { get; init; }
    public required Modalidad Modalidad { get; init; }
    public required string Cuis { get; init; }
    public required string Cufd { get; init; }
    public required int CodigoSucursal { get; init; }
    public required int CodigoPuntoVenta { get; init; }
    public required int CodigoEvento { get; init; }
    public required DateTime FechaHoraEvento { get; init; }
    public required string DescripcionEvento { get; init; }
}

public sealed record SincronizacionRequest
{
    public required long Nit { get; init; }
    public required string CodigoSistema { get; init; }
    public required Ambiente Ambiente { get; init; }
    public required Modalidad Modalidad { get; init; }
    public required string Cuis { get; init; }
    public int CodigoSucursal { get; init; } = 0;
    public int CodigoPuntoVenta { get; init; } = 0;
}

/// <summary>Solicitud de una paramétrica de sincronización (catálogo).</summary>
public sealed record ParametricaRequest
{
    public required long Nit { get; init; }
    public required string CodigoSistema { get; init; }
    public required string Cuis { get; init; }
    public required Ambiente Ambiente { get; init; }
    public int CodigoSucursal { get; init; } = 0;
    public int CodigoPuntoVenta { get; init; } = 0;
}

/// <summary>Solicitud de consulta de puntos de venta de una sucursal.</summary>
public sealed record PuntosVentaRequest
{
    public required long Nit { get; init; }
    public required string CodigoSistema { get; init; }
    public required string Cuis { get; init; }
    public required Ambiente Ambiente { get; init; }
    public int CodigoSucursal { get; init; } = 0;
}

/// <summary>Solicitud de registro de un punto de venta en una sucursal.</summary>
public sealed record RegistroPuntoVentaRequest
{
    public required long Nit { get; init; }
    public required string CodigoSistema { get; init; }
    public required string Cuis { get; init; }
    public required Ambiente Ambiente { get; init; }
    public required Modalidad Modalidad { get; init; }
    public required int CodigoSucursal { get; init; }
    public required int CodigoTipoPuntoVenta { get; init; }
    public required string NombrePuntoVenta { get; init; }
    public string Descripcion { get; init; } = string.Empty;
}

/// <summary>Solicitud de cierre de un punto de venta.</summary>
public sealed record CierrePuntoVentaRequest
{
    public required long Nit { get; init; }
    public required string CodigoSistema { get; init; }
    public required string Cuis { get; init; }
    public required Ambiente Ambiente { get; init; }
    public required int CodigoSucursal { get; init; }
    public required int CodigoPuntoVenta { get; init; }
}

public sealed record HomologacionRequest
{
    public required long Nit { get; init; }
    public required string CodigoSistema { get; init; }
    public required Ambiente Ambiente { get; init; }
    public required Modalidad Modalidad { get; init; }
    public required string Cuis { get; init; }
    public int CodigoSucursal { get; init; } = 0;
    public int CodigoPuntoVenta { get; init; } = 0;
    public int CodigoActividadEconomica { get; init; } = 0;
}