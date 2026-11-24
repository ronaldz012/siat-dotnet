namespace MiEmpresa.Siat.Sdk.Operations;

using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using MiEmpresa.Siat.Sdk.Clients;
using MiEmpresa.Siat.Sdk.Hashing;
using MiEmpresa.Siat.Sdk.Models;
using MiEmpresa.Siat.Sdk.Models.Requests;
using MiEmpresa.Siat.Sdk.Models.Responses;
using MiEmpresa.Siat.Sdk.Xml;

public sealed class EmitirFacturaComputarizadaOperation
{
    private readonly ISiatClient _client;

    public EmitirFacturaComputarizadaOperation(ISiatClient client)
    {
        _client = client;
    }

    public async Task<EmisionResponse> EjecutarAsync(FacturaRequest request, string token, CancellationToken ct)
    {
        var facturaXml = ConstruirFacturaXml(request);
        var (hashArchivo, archivoBase64) = HashCalculator.ComprimirYHashear(facturaXml);

        var soapRequest = new FacturaSoapRequest
        {
            SolicitudServicioRecepcionFactura = new SolicitudServicioRecepcionFactura
            {
                SolicitudRecepcion = new SolicitudRecepcion
                {
                    CodigoAmbiente = (int)request.Cabecera.Ambiente,
                    CodigoModalidad = (int)Modalidad.ComputarizadaEnLinea,
                    CodigoSistema = request.Cabecera.CodigoSistema,
                    Nit = request.Cabecera.NitEmisor,
                    CodigoSucursal = request.Cabecera.CodigoSucursal,
                    CodigoPuntoVenta = request.Cabecera.CodigoPuntoVenta,
                    CodigoDocumentoSector = request.Cabecera.CodigoDocumentoSector,
                    TipoFacturaDocumento = (int)request.Cabecera.TipoFacturaDocumento,
                    CodigoEmision = (int)request.Cabecera.CodigoEmision,
                    Cuis = request.Cabecera.Cuis,
                    Cufd = request.Cabecera.Cufd,
                    FechaEnvio = DateTime.Now,
                    Archivo = archivoBase64,
                    HashArchivo = hashArchivo
                }
            }
        };

        var serviceName = ObtenerServicioFacturacion(request.Cabecera.CodigoDocumentoSector);

        var response = await _client.PostSoapAsync<FacturaSoapRequest, FacturaSoapResponse>(
            serviceName,
            soapRequest,
            token,
            request.Cabecera.Ambiente,
            ct);

        return MapearRespuesta(response);
    }

    private static string ConstruirFacturaXml(FacturaRequest request)
    {
        var factura = new FacturaComputarizada
        {
            Cabecera = MapearCabecera(request.Cabecera),
            Detalle = request.Detalles.Select(MapearDetalle).ToArray()
        };

        var serializer = new XmlSerializer(typeof(FacturaComputarizada));
        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = true,
            Encoding = Encoding.UTF8
        });
        serializer.Serialize(xmlWriter, factura);
        return stringWriter.ToString();
    }

    private static FacturaCabeceraXml MapearCabecera(FacturaCabeceraRequest c)
    {
        return new FacturaCabeceraXml
        {
            NitEmisor = c.NitEmisor,
            RazonSocialEmisor = c.RazonSocialEmisor,
            Municipio = c.Municipio,
            Telefono = c.Telefono,
            NumeroFactura = c.NumeroFactura,
            Cufd = c.Cufd,
            CodigoSucursal = c.CodigoSucursal,
            CodigoPuntoVenta = c.CodigoPuntoVenta,
            FechaEmision = c.FechaEmision.ToString("yyyy-MM-ddTHH:mm:ss"),
            NombreRazonSocial = c.NombreRazonSocialCliente,
            CodigoTipoDocumentoIdentidad = (int)c.CodigoTipoDocumentoIdentidad,
            NumeroDocumento = c.NumeroDocumentoCliente,
            CodigoCliente = c.CodigoCliente,
            CodigoMetodoPago = (int)c.CodigoMetodoPago,
            MontoTotal = c.MontoTotal,
            CodigoMoneda = (int)c.CodigoMoneda,
            TipoCambio = c.TipoCambio,
            MontoTotalMoneda = c.MontoTotalMoneda,
            MontoTotalSujetoIva = c.MontoTotalSujetoIva,
            CodigoDocumentoSector = c.CodigoDocumentoSector,
            TipoFacturaDocumento = (int)c.TipoFacturaDocumento,
            CodigoEmision = (int)c.CodigoEmision,
            MontoGiftCard = c.MontoGiftCard,
            DescuentoAdicional = c.DescuentoAdicional,
            CodigoExcepcion = c.CodigoExcepcion,
            Ccafc = c.Ccafc,
            Leyenda = c.Leyenda,
            Usuario = c.Usuario,
            Direccion = c.Direccion,
            Complemento = c.Complemento
        };
    }

    private static FacturaDetalleXml MapearDetalle(FacturaDetalleRequest d)
    {
        return new FacturaDetalleXml
        {
            ActividadEconomica = d.ActividadEconomica,
            CodigoProductoSin = d.CodigoProductoSin,
            CodigoUnidadMedida = d.CodigoUnidadMedida,
            Cantidad = d.Cantidad,
            CodigoProducto = d.CodigoProducto,
            Descripcion = d.Descripcion,
            PrecioUnitario = d.PrecioUnitario,
            MontoDescuento = d.MontoDescuento,
            SubTotal = d.SubTotal,
            NumeroImei = d.NumeroImei,
            NumeroSerie = d.NumeroSerie
        };
    }

    private static string ObtenerServicioFacturacion(int codigoDocumentoSector)
    {
        return codigoDocumentoSector switch
        {
            13 => "ServicioFacturacionServicioBasico",
            1 => "ServicioFacturacionCompraVenta",
            15 => "ServicioFacturacionEntidadFinanciera",
            22 => "ServicioFacturacionTelecomunicaciones",
            24 => "ServicioFacturacionDocumentoAjuste",
            _ => "ServicioFacturacionComputarizada"
        };
    }

    private static EmisionResponse MapearRespuesta(FacturaSoapResponse soapResp)
    {
        var resp = soapResp.RespuestaServicioFacturacion;
        return new EmisionResponse
        {
            Transaccion = resp.Transaccion,
            CodigoRecepcion = resp.CodigoRecepcion,
            Cuf = resp.Cuf,
            Mensajes = (resp.MensajesList ?? Array.Empty<SiatMensajeSoap>())
                .Select(m => new SiatMensaje { Codigo = m.Codigo, Descripcion = m.Descripcion })
                .ToList()
        };
    }
}

[XmlRoot("SolicitudServicioRecepcionFactura", Namespace = "https://siat.impuestos.gob.bo/")]
public sealed class FacturaSoapRequest
{
    [XmlElement("SolicitudServicioRecepcionFactura")]
    public SolicitudServicioRecepcionFactura SolicitudServicioRecepcionFactura { get; set; } = new();
}

public sealed class SolicitudServicioRecepcionFactura
{
    [XmlElement("SolicitudRecepcion")]
    public SolicitudRecepcion SolicitudRecepcion { get; set; } = new();
}

public sealed class SolicitudRecepcion
{
    [XmlElement("codigoAmbiente")]
    public int CodigoAmbiente { get; set; }

    [XmlElement("codigoModalidad")]
    public int CodigoModalidad { get; set; }

    [XmlElement("codigoSistema")]
    public string CodigoSistema { get; set; } = string.Empty;

    [XmlElement("nit")]
    public long Nit { get; set; }

    [XmlElement("codigoSucursal")]
    public int CodigoSucursal { get; set; }

    [XmlElement("codigoPuntoVenta")]
    public int CodigoPuntoVenta { get; set; }

    [XmlElement("codigoDocumentoSector")]
    public int CodigoDocumentoSector { get; set; }

    [XmlElement("tipoFacturaDocumento")]
    public int TipoFacturaDocumento { get; set; }

    [XmlElement("codigoEmision")]
    public int CodigoEmision { get; set; }

    [XmlElement("cuis")]
    public string Cuis { get; set; } = string.Empty;

    [XmlElement("cufd")]
    public string Cufd { get; set; } = string.Empty;

    [XmlElement("fechaEnvio")]
    public DateTime FechaEnvio { get; set; }

    [XmlElement("archivo")]
    public string Archivo { get; set; } = string.Empty;

    [XmlElement("hashArchivo")]
    public string HashArchivo { get; set; } = string.Empty;
}

[XmlRoot("RespuestaServicioFacturacion", Namespace = "https://siat.impuestos.gob.bo/")]
public sealed class FacturaSoapResponse
{
    [XmlElement("RespuestaServicioFacturacion")]
    public RespuestaServicioFacturacionSoap RespuestaServicioFacturacion { get; set; } = new();
}

public sealed class RespuestaServicioFacturacionSoap
{
    [XmlElement("transaccion")]
    public bool Transaccion { get; set; }

    [XmlElement("codigoRecepcion")]
    public string? CodigoRecepcion { get; set; }

    [XmlElement("cuf")]
    public string? Cuf { get; set; }

    [XmlArray("mensajesList")]
    [XmlArrayItem("mensaje")]
    public SiatMensajeSoap[] MensajesList { get; set; } = Array.Empty<SiatMensajeSoap>();
}

[XmlRoot("Factura", Namespace = "https://siat.impuestos.gob.bo/")]
public sealed class FacturaComputarizada
{
    [XmlElement("cabecera")]
    public FacturaCabeceraXml Cabecera { get; set; } = new();

    [XmlArray("detalle")]
    [XmlArrayItem("detalle")]
    public FacturaDetalleXml[] Detalle { get; set; } = Array.Empty<FacturaDetalleXml>();
}

public sealed class FacturaCabeceraXml
{
    [XmlElement("nitEmisor")]
    public long NitEmisor { get; set; }

    [XmlElement("razonSocialEmisor")]
    public string RazonSocialEmisor { get; set; } = string.Empty;

    [XmlElement("municipio")]
    public string Municipio { get; set; } = string.Empty;

    [XmlElement("telefono")]
    public string Telefono { get; set; } = string.Empty;

    [XmlElement("numeroFactura")]
    public int NumeroFactura { get; set; }

    [XmlElement("cufd")]
    public string Cufd { get; set; } = string.Empty;

    [XmlElement("codigoSucursal")]
    public int CodigoSucursal { get; set; }

    [XmlElement("codigoPuntoVenta")]
    public int CodigoPuntoVenta { get; set; }

    [XmlElement("fechaEmision")]
    public string FechaEmision { get; set; } = string.Empty;

    [XmlElement("nombreRazonSocial")]
    public string NombreRazonSocial { get; set; } = string.Empty;

    [XmlElement("codigoTipoDocumentoIdentidad")]
    public int CodigoTipoDocumentoIdentidad { get; set; }

    [XmlElement("numeroDocumento")]
    public string NumeroDocumento { get; set; } = string.Empty;

    [XmlElement("codigoCliente")]
    public string CodigoCliente { get; set; } = string.Empty;

    [XmlElement("codigoMetodoPago")]
    public int CodigoMetodoPago { get; set; }

    [XmlElement("montoTotal")]
    public decimal MontoTotal { get; set; }

    [XmlElement("codigoMoneda")]
    public int CodigoMoneda { get; set; }

    [XmlElement("tipoCambio")]
    public decimal TipoCambio { get; set; }

    [XmlElement("montoTotalMoneda")]
    public decimal MontoTotalMoneda { get; set; }

    [XmlElement("montoTotalSujetoIva")]
    public decimal MontoTotalSujetoIva { get; set; }

    [XmlElement("codigoDocumentoSector")]
    public int CodigoDocumentoSector { get; set; }

    [XmlElement("tipoFacturaDocumento")]
    public int TipoFacturaDocumento { get; set; }

    [XmlElement("codigoEmision")]
    public int CodigoEmision { get; set; }

    [XmlElement("montoGiftCard")]
    public decimal? MontoGiftCard { get; set; }

    [XmlElement("descuentoAdicional")]
    public decimal? DescuentoAdicional { get; set; }

    [XmlElement("codigoExcepcion")]
    public string? CodigoExcepcion { get; set; }

    [XmlElement("cafc")]
    public string? Ccafc { get; set; }

    [XmlElement("leyenda")]
    public string? Leyenda { get; set; }

    [XmlElement("usuario")]
    public string? Usuario { get; set; }

    [XmlElement("direccion")]
    public string? Direccion { get; set; }

    [XmlElement("complemento")]
    public string? Complemento { get; set; }
}

public sealed class FacturaDetalleXml
{
    [XmlElement("actividadEconomica")]
    public int ActividadEconomica { get; set; }

    [XmlElement("codigoProductoSin")]
    public int CodigoProductoSin { get; set; }

    [XmlElement("codigoUnidadMedida")]
    public int CodigoUnidadMedida { get; set; }

    [XmlElement("cantidad")]
    public decimal Cantidad { get; set; }

    [XmlElement("codigoProducto")]
    public string CodigoProducto { get; set; } = string.Empty;

    [XmlElement("descripcion")]
    public string Descripcion { get; set; } = string.Empty;

    [XmlElement("precioUnitario")]
    public decimal PrecioUnitario { get; set; }

    [XmlElement("montoDescuento")]
    public decimal? MontoDescuento { get; set; }

    [XmlElement("subTotal")]
    public decimal SubTotal { get; set; }

    [XmlElement("numeroImei")]
    public string? NumeroImei { get; set; }

    [XmlElement("numeroSerie")]
    public string? NumeroSerie { get; set; }
}