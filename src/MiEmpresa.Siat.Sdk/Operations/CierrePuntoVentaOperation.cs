namespace MiEmpresa.Siat.Sdk.Operations;

using System.Xml.Serialization;

using MiEmpresa.Siat.Sdk.Clients;
using MiEmpresa.Siat.Sdk.Models.Requests;
using MiEmpresa.Siat.Sdk.Models.Responses;
using MiEmpresa.Siat.Sdk.Xml;

/// <summary>
/// Cierra un punto de venta en el SIN (`cierrePuntoVenta`, servicio
/// <c>FacturacionOperaciones</c>). El CUIS es el de la sucursal (pv=0).
/// </summary>
public sealed class CierrePuntoVentaOperation(ISiatClient client)
{
    public async Task<CierrePuntoVentaResponse> EjecutarAsync(
        CierrePuntoVentaRequest request, string token, CancellationToken ct)
    {
        var payload = new CierrePuntoVentaSoap
        {
            CodigoAmbiente = (int)request.Ambiente,
            CodigoPuntoVenta = request.CodigoPuntoVenta,
            CodigoSistema = request.CodigoSistema,
            CodigoSucursal = request.CodigoSucursal,
            Cuis = request.Cuis,
            Nit = request.Nit,
        };

        var xml = SoapEnvelopeBuilder.BuildEnvelope("cierrePuntoVenta", payload);

        var response = await client.PostSoapXmlAsync<CierrePuntoVentaSoapResponse>(
            "FacturacionOperaciones", xml, token, request.Ambiente, payloadOnly: true, ct);

        return new CierrePuntoVentaResponse
        {
            Transaccion = response.Transaccion,
            CodigoPuntoVenta = response.CodigoPuntoVenta,
            Mensajes = response.MensajesList
                .Select(m => new SiatMensaje { Codigo = m.Codigo, Descripcion = m.Descripcion })
                .ToList(),
        };
    }
}

[XmlRoot("SolicitudCierrePuntoVenta")]
public sealed class CierrePuntoVentaSoap
{
    [XmlElement("codigoAmbiente")]
    public int CodigoAmbiente { get; set; }

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

[XmlRoot("RespuestaCierrePuntoVenta")]
public sealed class CierrePuntoVentaSoapResponse
{
    [XmlElement("codigoPuntoVenta", Namespace = "")]
    public int CodigoPuntoVenta { get; set; }

    [XmlElement("mensajesList", Namespace = "")]
    public List<SiatMensajeSoap> MensajesList { get; set; } = [];

    [XmlElement("transaccion", Namespace = "")]
    public bool Transaccion { get; set; }
}
