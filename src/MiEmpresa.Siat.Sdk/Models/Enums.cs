namespace MiEmpresa.Siat.Sdk.Models;

public enum Ambiente
{
    Produccion = 1,
    Pruebas = 2
}

public enum Modalidad
{
    ElectronicaEnLinea = 1,
    ComputarizadaEnLinea = 2
}

public enum TipoEmision
{
    Online = 1,
    Offline = 2,
    Masiva = 3
}

public enum TipoFacturaDocumento
{
    ConDerechoCreditoFiscal = 1,
    SinDerechoCreditoFiscal = 2,
    DocumentoAjuste = 3
}

public enum CodigoMotivoAnulacion
{
    ErrorEnEmision = 1,
    ErrorEnDatosDelCliente = 2,
    ErrorEnDatosDeLaFactura = 3,
    DevolucionDeMercaderia = 4,
    AnulacionPorConvenio = 5,
    Otro = 99
}

public enum DocumentoSector
{
    CompraVenta = 1,
    AlquilerBienesInmuebles = 2,
    ComercialExportacion = 3,
    ComercialExportacionLibreConsignacion = 4,
    VentaZonaFranca = 5,
    ServicioTuristico = 6,
    SegAlimentaria = 7,
    TasaCeroLibros = 8,
    CompraVentaMonedaExtranjera = 9,
    DuttyFree = 10,
    SectorEducativo = 11,
    ComHidrocarburos = 12,
    ServBasicos = 13,
    ProdsIce = 14,
    EntFinanciera = 15,
    Hoteles = 16,
    Hospitales = 17,
    JuegosAzar = 18,
    Hidrocarburos = 19,
    ExportMinerales = 20,
    VentaMinerales = 21,
    Telecomunicaciones = 22,
    Prevalorada = 23,
    NotaCredDeb = 24,
    ComExportServicios = 28,
    NotaConciliacion = 29,
    BoletoAereo = 30,
    SuminEnergia = 31,
    IceZonaFranca = 32
}

public enum TipoDocumentoIdentidad
{
    Nit = 1,
    CarnetIdentidad = 2,
    Pasaporte = 3,
    LicenciaConducir = 4,
    Otro = 5
}

public enum CodigoMetodoPago
{
    Efectivo = 1,
    TarjetaDebito = 2,
    TarjetaCredito = 3,
    Cheque = 4,
    Transferencia = 5,
    Otros = 6,
    MonederoElectronico = 7,
    TarjetaRegalo = 8,
    CodigoQr = 9
}

public enum CodigoMoneda
{
    Boliviano = 1,
    DolarAmericano = 2
}

public enum CodigoPuntoVenta
{
    NoAplica = 0
}

public enum CodigoSucursal
{
    CasaMatriz = 0
}

/// <summary>
/// Catálogos paramétricos estándar de <c>FacturacionSincronizacion</c> (respuesta
/// `RespuestaListaParametricas`). El nombre coincide con el de la operación SOAP; el
/// mapeo vive en el SDK, no en los consumidores.
/// </summary>
public enum ParametricaSiat
{
    TipoPuntoVenta,
    UnidadMedida,
    TipoDocumentoIdentidad,
    TipoMetodoPago,
    TipoMoneda,
    TiposFactura,
    TipoDocumentoSector,
    TipoEmision,
    MotivoAnulacion,
    EventosSignificativos,
    PaisOrigen,
    TipoHabitacion,
    MensajesServicios,
}