namespace MiEmpresa.Siat.Sdk.Operations;
using System.Xml.Serialization;

using MiEmpresa.Siat.Sdk.Clients;
using MiEmpresa.Siat.Sdk.Models.Requests;
using MiEmpresa.Siat.Sdk.Models.Responses;

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
            FechaVigencia = resp.FechaVigencia,
            Transaccion = resp.Transaccion,
            Mensajes = (resp.MensajesList ?? Array.Empty<SiatMensajeSoap>())
                .Select(m => new SiatMensaje { Codigo = m.Codigo, Descripcion = m.Descripcion })
                .ToList()
        };
    }
}

[XmlRoot("SolicitudCufd", Namespace = "https://siat.impuestos.gob.bo/")]
public sealed class CufdSoapRequest
{
    [XmlElement("SolicitudCufd")]
    public SolicitudCufd SolicitudCufd { get; set; } = new();
}

public sealed class SolicitudCufd
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

    [XmlElement("cuis")]
    public string Cuis { get; set; } = string.Empty;

    [XmlElement("nit")]
    public long Nit { get; set; }
}

[XmlRoot("RespuestaCufd", Namespace = "https://siat.impuestos.gob.bo/")]
public sealed class CufdSoapResponse
{
    [XmlElement("RespuestaCufd")]
    public RespuestaCufdSoap RespuestaCufd { get; set; } = new();
}

public sealed class RespuestaCufdSoap
{
    [XmlElement("codigo")]
    public string? Codigo { get; set; }

    [XmlElement("codigoControl")]
    public string? CodigoControl { get; set; }

    [XmlElement("direccion")]
    public string? Direccion { get; set; }

    [XmlElement("fechaVigencia")]
    public DateTime FechaVigencia { get; set; }

    [XmlElement("transaccion")]
    public bool Transaccion { get; set; }

    [XmlArray("mensajesList")]
    [XmlArrayItem("mensaje")]
    public SiatMensajeSoap[] MensajesList { get; set; } = Array.Empty<SiatMensajeSoap>();
}