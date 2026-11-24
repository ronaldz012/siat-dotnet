namespace MiEmpresa.Siat.Sdk.Operations;
using System.Xml.Serialization;

using MiEmpresa.Siat.Sdk.Clients;
using MiEmpresa.Siat.Sdk.Models;
using MiEmpresa.Siat.Sdk.Models.Requests;
using MiEmpresa.Siat.Sdk.Models.Responses;
using MiEmpresa.Siat.Sdk.Xml;

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
            request.Ambiente,
            ct);

        return MapearRespuesta(response);
    }

    private static CuisResponse MapearRespuesta(CuisSoapResponse soapResp)
    {
        var resp = soapResp.RespuestaCuis;
        return new CuisResponse
        {
            Codigo = resp.Codigo ?? string.Empty,
            FechaVigencia = SiatDate.ToUtc(resp.FechaVigencia),
            CodigoControl = resp.CodigoControl ?? string.Empty,
            Transaccion = resp.Transaccion,
            Mensajes = (resp.MensajesList ?? Array.Empty<SiatMensajeSoap>())
                .Select(m => new SiatMensaje { Codigo = m.Codigo, Descripcion = m.Descripcion })
                .ToList()
        };
    }
}

/// <summary>Elemento de la operación &lt;ns:cuis&gt; con su payload &lt;SolicitudCuis&gt;.</summary>
[XmlRoot("cuis", Namespace = SoapEnvelopeBuilder.SiatNamespace)]
public sealed class CuisSoapRequest
{
    [XmlElement("SolicitudCuis", Namespace = "")]
    public SolicitudCuis SolicitudCuis { get; set; } = new();
}

public sealed class SolicitudCuis
{
    [XmlElement("codigoAmbiente", Namespace = "")]
    public int CodigoAmbiente { get; set; }

    [XmlElement("codigoModalidad", Namespace = "")]
    public int CodigoModalidad { get; set; }

    [XmlElement("codigoPuntoVenta", Namespace = "")]
    public int CodigoPuntoVenta { get; set; }

    [XmlElement("codigoSistema", Namespace = "")]
    public string CodigoSistema { get; set; } = string.Empty;

    [XmlElement("codigoSucursal", Namespace = "")]
    public int CodigoSucursal { get; set; }

    [XmlElement("nit", Namespace = "")]
    public long Nit { get; set; }
}

/// <summary>Elemento &lt;ns:cuisResponse&gt; que envuelve la &lt;RespuestaCuis&gt;.</summary>
[XmlRoot("cuisResponse")]
public sealed class CuisSoapResponse
{
    [XmlElement("RespuestaCuis", Namespace = "")]
    public RespuestaCuisSoap RespuestaCuis { get; set; } = new();
}

public sealed class RespuestaCuisSoap
{
    [XmlElement("codigo", Namespace = "")]
    public string? Codigo { get; set; }

    [XmlElement("fechaVigencia", Namespace = "")]
    public DateTime FechaVigencia { get; set; }

    [XmlElement("codigoControl", Namespace = "")]
    public string? CodigoControl { get; set; }

    [XmlElement("transaccion", Namespace = "")]
    public bool Transaccion { get; set; }

    [XmlElement("mensajesList", Namespace = "")]
    public SiatMensajeSoap[] MensajesList { get; set; } = Array.Empty<SiatMensajeSoap>();
}

public sealed class SiatMensajeSoap
{
    [XmlElement("codigo", Namespace = "")]
    public int Codigo { get; set; }

    [XmlElement("descripcion", Namespace = "")]
    public string Descripcion { get; set; } = string.Empty;
}
