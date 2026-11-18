namespace MiEmpresa.Siat.Sdk.Operations;
using System.Xml.Serialization;

using MiEmpresa.Siat.Sdk.Clients;
using MiEmpresa.Siat.Sdk.Models.Requests;
using MiEmpresa.Siat.Sdk.Models.Responses;

public sealed class SincronizarCatalogosOperation
{
    private readonly ISiatClient _client;

    public SincronizarCatalogosOperation(ISiatClient client)
    {
        _client = client;
    }

    public async Task<SincronizacionResponse> EjecutarAsync(SincronizacionRequest request, string token, CancellationToken ct)
    {
        var soapRequest = new SincronizacionSoapRequest
        {
            SolicitudSincronizacion = new SolicitudSincronizacion
            {
                CodigoAmbiente = (int)request.Ambiente,
                CodigoModalidad = (int)request.Modalidad,
                CodigoSistema = request.CodigoSistema,
                Nit = request.Nit,
                CodigoSucursal = request.CodigoSucursal,
                CodigoPuntoVenta = request.CodigoPuntoVenta
            }
        };

        var response = await _client.PostSoapAsync<SincronizacionSoapRequest, SincronizacionSoapResponse>(
            "FacturacionSincronizacion",
            soapRequest,
            token,
            ct);

        return MapearRespuesta(response);
    }

    private static SincronizacionResponse MapearRespuesta(SincronizacionSoapResponse soapResp)
    {
        var resp = soapResp.RespuestaSincronizacion;
        return new SincronizacionResponse
        {
            Transaccion = resp.Transaccion,
            Mensajes = (resp.MensajesList ?? Array.Empty<SiatMensajeSoap>())
                .Select(m => new SiatMensaje { Codigo = m.Codigo, Descripcion = m.Descripcion })
                .ToList(),
            Datos = resp.ListaActividadesEconomicas ?? resp.ListaProductosServicios ?? resp.ListaDocumentosSector ?? resp.ListaParametricas
        };
    }
}

[XmlRoot("SolicitudSincronizacion", Namespace = "https://siat.impuestos.gob.bo/")]
public sealed class SincronizacionSoapRequest
{
    [XmlElement("SolicitudSincronizacion")]
    public SolicitudSincronizacion SolicitudSincronizacion { get; set; } = new();
}

public sealed class SolicitudSincronizacion
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
}

[XmlRoot("RespuestaSincronizacion", Namespace = "https://siat.impuestos.gob.bo/")]
public sealed class SincronizacionSoapResponse
{
    [XmlElement("RespuestaSincronizacion")]
    public RespuestaSincronizacionSoap RespuestaSincronizacion { get; set; } = new();
}

public sealed class RespuestaSincronizacionSoap
{
    [XmlElement("transaccion")]
    public bool Transaccion { get; set; }

    [XmlElement("listaActividadesEconomicas")]
    public object? ListaActividadesEconomicas { get; set; }

    [XmlElement("listaProductosServicios")]
    public object? ListaProductosServicios { get; set; }

    [XmlElement("listaDocumentosSector")]
    public object? ListaDocumentosSector { get; set; }

    [XmlElement("listaParametricas")]
    public object? ListaParametricas { get; set; }

    [XmlArray("mensajesList")]
    [XmlArrayItem("mensaje")]
    public SiatMensajeSoap[] MensajesList { get; set; } = Array.Empty<SiatMensajeSoap>();
}