namespace MiEmpresa.Siat.Sdk.Operations;
using System.Xml.Serialization;

using MiEmpresa.Siat.Sdk.Clients;
using MiEmpresa.Siat.Sdk.Models.Requests;
using MiEmpresa.Siat.Sdk.Models.Responses;
using MiEmpresa.Siat.Sdk.Exceptions;

public sealed class ObtenerCuisOperation
{
    private readonly ISiatClient _client;

    public ObtenerCuisOperation(ISiatClient client)
    {
        _client = client;
    }

    public async Task<CuisResponse> EjecutarAsync(CuisRequest request, string token, CancellationToken ct)
    {
        var soapRequest = new CuisSoapRequest
        {
            SolicitudCuis = new SolicitudCuis
            {
                CodigoAmbiente = (int)request.Ambiente,
                CodigoModalidad = (int)request.Modalidad,
                CodigoPuntoVenta = request.CodigoPuntoVenta,
                CodigoSistema = request.CodigoSistema,
                CodigoSucursal = request.CodigoSucursal,
                Nit = request.Nit
            }
        };

        var response = await _client.PostSoapAsync<CuisSoapRequest, CuisSoapResponse>(
            "FacturacionCodigos",
            soapRequest,
            token,
            ct);

        return MapearRespuesta(response);
    }

private static CuisResponse MapearRespuesta(CuisSoapResponse soapResp)
{
    return new CuisResponse
    {
        Codigo = soapResp.Codigo ?? string.Empty,
        FechaVigencia = soapResp.FechaVigencia,
        CodigoControl = soapResp.CodigoControl ?? string.Empty,
        Transaccion = soapResp.Transaccion,
        Mensajes = (soapResp.MensajesList ?? Array.Empty<SiatMensajeSoap>())
            .Select(m => new SiatMensaje { Codigo = m.Codigo, Descripcion = m.Descripcion })
            .ToList()
    };
}
}

[XmlRoot("SolicitudCuis", Namespace = "https://siat.impuestos.gob.bo/")]
public sealed class CuisSoapRequest
{
    [XmlElement("SolicitudCuis")]
    public SolicitudCuis SolicitudCuis { get; set; } = new();
}

public sealed class SolicitudCuis
{
    [XmlElement("codigoAmbiente")]
    public int CodigoAmbiente { get; set; }

    [XmlElement("codigoModalidad")]
    public int CodigoModalidad { get; set; }

    [XmlElement("codigoPuntoVenta")]
    public int CodigoPuntoVenta { get; set; }

    [XmlElement("codigoSistema")]
    public string CodigoSistema { get; set; } = string.Empty;

    [XmlElement("codigoSucursal")]
    public int CodigoSucursal { get; set; }

    [XmlElement("nit")]
    public long Nit { get; set; }
}

[XmlRoot("RespuestaCuis", Namespace = "https://siat.impuestos.gob.bo/")]
public sealed class CuisSoapResponse
{
    [XmlElement("codigo", Namespace = "https://siat.impuestos.gob.bo/")]
    public string? Codigo { get; set; }

    [XmlElement("fechaVigencia", Namespace = "https://siat.impuestos.gob.bo/")]
    public DateTime FechaVigencia { get; set; }

    [XmlElement("codigoControl", Namespace = "https://siat.impuestos.gob.bo/")]
    public string? CodigoControl { get; set; }

    [XmlElement("transaccion", Namespace = "https://siat.impuestos.gob.bo/")]
    public bool Transaccion { get; set; }

    [XmlArray("mensajesList", Namespace = "https://siat.impuestos.gob.bo/")]
    [XmlArrayItem("mensaje", Namespace = "https://siat.impuestos.gob.bo/")]
    public SiatMensajeSoap[] MensajesList { get; set; } = Array.Empty<SiatMensajeSoap>();
}

public sealed class SiatMensajeSoap
{
    [XmlElement("codigo")]
    public int Codigo { get; set; }

    [XmlElement("descripcion")]
    public string Descripcion { get; set; } = string.Empty;
}