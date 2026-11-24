namespace MiEmpresa.Siat.Sdk.Operations;
using System.Xml.Serialization;

using MiEmpresa.Siat.Sdk.Clients;
using MiEmpresa.Siat.Sdk.Models;
using MiEmpresa.Siat.Sdk.Models.Requests;
using MiEmpresa.Siat.Sdk.Models.Responses;
using MiEmpresa.Siat.Sdk.Xml;

public sealed class ObtenerCufdOperation
{
    private readonly ISiatClient _client;

    public ObtenerCufdOperation(ISiatClient client)
    {
        _client = client;
    }

    public async Task<CufdResponse> EjecutarAsync(CufdRequest request, string token, CancellationToken ct)
    {
        var soapRequest = new CufdSoapRequest
        {
            SolicitudCufd = new SolicitudCufd
            {
                CodigoAmbiente = (int)request.Ambiente,
                CodigoModalidad = (int)request.Modalidad,
                CodigoPuntoVenta = request.CodigoPuntoVenta,
                CodigoSistema = request.CodigoSistema,
                CodigoSucursal = request.CodigoSucursal,
                Cuis = request.Cuis,
                Nit = request.Nit
            }
        };

        var response = await _client.PostSoapAsync<CufdSoapRequest, CufdSoapResponse>(
            "FacturacionCodigos",
            soapRequest,
            token,
            request.Ambiente,
            ct);

        return MapearRespuesta(response);
    }

    private static CufdResponse MapearRespuesta(CufdSoapResponse soapResp)
    {
        var resp = soapResp.RespuestaCufd;
        return new CufdResponse
        {
            Codigo = resp.Codigo ?? string.Empty,
            CodigoControl = resp.CodigoControl ?? string.Empty,
            Direccion = resp.Direccion ?? string.Empty,
            FechaVigencia = SiatDate.ToUtc(resp.FechaVigencia),
            Transaccion = resp.Transaccion,
            Mensajes = (resp.MensajesList ?? Array.Empty<SiatMensajeSoap>())
                .Select(m => new SiatMensaje { Codigo = m.Codigo, Descripcion = m.Descripcion })
                .ToList()
        };
    }
}

[XmlRoot("cufd", Namespace = SoapEnvelopeBuilder.SiatNamespace)]
public sealed class CufdSoapRequest
{
    [XmlElement("SolicitudCufd", Namespace = "")]
    public SolicitudCufd SolicitudCufd { get; set; } = new();
}

public sealed class SolicitudCufd
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

    [XmlElement("cuis", Namespace = "")]
    public string Cuis { get; set; } = string.Empty;

    [XmlElement("nit", Namespace = "")]
    public long Nit { get; set; }
}

[XmlRoot("cufdResponse")]
public sealed class CufdSoapResponse
{
    [XmlElement("RespuestaCufd", Namespace = "")]
    public RespuestaCufdSoap RespuestaCufd { get; set; } = new();
}

public sealed class RespuestaCufdSoap
{
    [XmlElement("codigo", Namespace = "")]
    public string? Codigo { get; set; }

    [XmlElement("codigoControl", Namespace = "")]
    public string? CodigoControl { get; set; }

    [XmlElement("direccion", Namespace = "")]
    public string? Direccion { get; set; }

    [XmlElement("fechaVigencia", Namespace = "")]
    public DateTime FechaVigencia { get; set; }

    [XmlElement("transaccion", Namespace = "")]
    public bool Transaccion { get; set; }

    [XmlElement("mensajesList", Namespace = "")]
    public SiatMensajeSoap[] MensajesList { get; set; } = Array.Empty<SiatMensajeSoap>();
}