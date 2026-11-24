namespace MiEmpresa.Siat.Sdk.Operations;
using System.Xml.Serialization;

using MiEmpresa.Siat.Sdk.Clients;
using MiEmpresa.Siat.Sdk.Models.Requests;
using MiEmpresa.Siat.Sdk.Models.Responses;

public sealed class RegistrarEventoOperation
{
    private readonly ISiatClient _client;

    public RegistrarEventoOperation(ISiatClient client)
    {
        _client = client;
    }

    public async Task<RegistroEventoResponse> EjecutarAsync(EventoSignificativoRequest request, string token, CancellationToken ct)
    {
        var soapRequest = new EventoSoapRequest
        {
            SolicitudEventoSignificativo = new SolicitudEventoSignificativo
            {
                CodigoAmbiente = (int)request.Ambiente,
                CodigoModalidad = (int)request.Modalidad,
                CodigoSistema = request.CodigoSistema,
                Nit = request.Nit,
                CodigoSucursal = request.CodigoSucursal,
                CodigoPuntoVenta = request.CodigoPuntoVenta,
                CodigoEvento = request.CodigoEvento,
                FechaHoraEvento = request.FechaHoraEvento,
                DescripcionEvento = request.DescripcionEvento,
                Cuis = request.Cuis,
                Cufd = request.Cufd
            }
        };

        var response = await _client.PostSoapAsync<EventoSoapRequest, EventoSoapResponse>(
            "FacturacionOperaciones",
            soapRequest,
            token,
            request.Ambiente,
            ct);

        return MapearRespuesta(response);
    }

    private static RegistroEventoResponse MapearRespuesta(EventoSoapResponse soapResp)
    {
        var resp = soapResp.RespuestaEventoSignificativo;
        return new RegistroEventoResponse
        {
            Transaccion = resp.Transaccion,
            CodigoEvento = resp.CodigoEvento,
            Mensajes = (resp.MensajesList ?? Array.Empty<SiatMensajeSoap>())
                .Select(m => new SiatMensaje { Codigo = m.Codigo, Descripcion = m.Descripcion })
                .ToList()
        };
    }
}

[XmlRoot("SolicitudEventoSignificativo", Namespace = "https://siat.impuestos.gob.bo/")]
public sealed class EventoSoapRequest
{
    [XmlElement("SolicitudEventoSignificativo")]
    public SolicitudEventoSignificativo SolicitudEventoSignificativo { get; set; } = new();
}

public sealed class SolicitudEventoSignificativo
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

    [XmlElement("codigoEvento")]
    public int CodigoEvento { get; set; }

    [XmlElement("fechaHoraEvento")]
    public DateTime FechaHoraEvento { get; set; }

    [XmlElement("descripcionEvento")]
    public string DescripcionEvento { get; set; } = string.Empty;

    [XmlElement("cuis")]
    public string Cuis { get; set; } = string.Empty;

    [XmlElement("cufd")]
    public string Cufd { get; set; } = string.Empty;
}

[XmlRoot("RespuestaEventoSignificativo", Namespace = "https://siat.impuestos.gob.bo/")]
public sealed class EventoSoapResponse
{
    [XmlElement("RespuestaEventoSignificativo")]
    public RespuestaEventoSoap RespuestaEventoSignificativo { get; set; } = new();
}

public sealed class RespuestaEventoSoap
{
    [XmlElement("transaccion")]
    public bool Transaccion { get; set; }

    [XmlElement("codigoEvento")]
    public int? CodigoEvento { get; set; }

    [XmlArray("mensajesList")]
    [XmlArrayItem("mensaje")]
    public SiatMensajeSoap[] MensajesList { get; set; } = Array.Empty<SiatMensajeSoap>();
}