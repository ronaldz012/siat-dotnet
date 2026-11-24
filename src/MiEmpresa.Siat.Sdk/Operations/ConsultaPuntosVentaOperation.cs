namespace MiEmpresa.Siat.Sdk.Operations;

using System.Xml.Serialization;

using MiEmpresa.Siat.Sdk.Clients;
using MiEmpresa.Siat.Sdk.Models.Requests;
using MiEmpresa.Siat.Sdk.Models.Responses;
using MiEmpresa.Siat.Sdk.Xml;

/// <summary>
/// Consulta los puntos de venta registrados en el SIN para una sucursal
/// (`consultaPuntoVenta`, servicio <c>FacturacionOperaciones</c>).
/// </summary>
public sealed class ConsultaPuntosVentaOperation(ISiatClient client)
{
    public async Task<ConsultaPuntosVentaResponse> EjecutarAsync(
        PuntosVentaRequest request, string token, CancellationToken ct)
    {
        var payload = new ConsultaPuntosVentaSoap
        {
            CodigoAmbiente = (int)request.Ambiente,
            CodigoSistema = request.CodigoSistema,
            CodigoSucursal = request.CodigoSucursal,
            Cuis = request.Cuis,
            Nit = request.Nit,
        };

        var xml = SoapEnvelopeBuilder.BuildEnvelope("consultaPuntoVenta", payload);

        var response = await client.PostSoapXmlAsync<ConsultaPuntosVentaSoapResponse>(
            "FacturacionOperaciones", xml, token, request.Ambiente, payloadOnly: true, ct);

        return new ConsultaPuntosVentaResponse
        {
            Transaccion = response.Transaccion,
            PuntosVenta = response.ListaPuntosVentas
                .Select(p => new PuntoVentaSiat
                {
                    CodigoPuntoVenta = p.CodigoPuntoVenta,
                    NombrePuntoVenta = p.NombrePuntoVenta,
                    TipoPuntoVenta = p.TipoPuntoVenta,
                })
                .ToList(),
            Mensajes = response.MensajesList
                .Select(m => new SiatMensaje { Codigo = m.Codigo, Descripcion = m.Descripcion })
                .ToList(),
        };
    }
}

[XmlRoot("SolicitudConsultaPuntoVenta")]
public sealed class ConsultaPuntosVentaSoap
{
    [XmlElement("codigoAmbiente")]
    public int CodigoAmbiente { get; set; }

    [XmlElement("codigoSistema")]
    public string CodigoSistema { get; set; } = string.Empty;

    [XmlElement("codigoSucursal")]
    public int CodigoSucursal { get; set; }

    [XmlElement("cuis")]
    public string Cuis { get; set; } = string.Empty;

    [XmlElement("nit")]
    public long Nit { get; set; }
}

[XmlRoot("RespuestaConsultaPuntoVenta")]
public sealed class ConsultaPuntosVentaSoapResponse
{
    [XmlElement("listaPuntosVentas", Namespace = "")]
    public List<PuntoVentaSoap> ListaPuntosVentas { get; set; } = [];

    [XmlElement("mensajesList", Namespace = "")]
    public List<SiatMensajeSoap> MensajesList { get; set; } = [];

    [XmlElement("transaccion", Namespace = "")]
    public bool Transaccion { get; set; }
}

public sealed class PuntoVentaSoap
{
    [XmlElement("codigoPuntoVenta", Namespace = "")]
    public int CodigoPuntoVenta { get; set; }

    [XmlElement("nombrePuntoVenta", Namespace = "")]
    public string NombrePuntoVenta { get; set; } = string.Empty;

    [XmlElement("tipoPuntoVenta", Namespace = "")]
    public string TipoPuntoVenta { get; set; } = string.Empty;
}
