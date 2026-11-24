namespace MiEmpresa.Siat.Sdk.Operations;

using System.Xml.Serialization;

using MiEmpresa.Siat.Sdk.Clients;
using MiEmpresa.Siat.Sdk.Models.Requests;
using MiEmpresa.Siat.Sdk.Models.Responses;
using MiEmpresa.Siat.Sdk.Xml;

/// <summary>
/// Registra un punto de venta en una sucursal (`registroPuntoVenta`, servicio
/// <c>FacturacionOperaciones</c>). El SIN asigna el código del PV.
/// </summary>
public sealed class RegistroPuntoVentaOperation(ISiatClient client)
{
    public async Task<RegistroPuntoVentaResponse> EjecutarAsync(
        RegistroPuntoVentaRequest request, string token, CancellationToken ct)
    {
        var payload = new RegistroPuntoVentaSoap
        {
            CodigoAmbiente = (int)request.Ambiente,
            CodigoModalidad = (int)request.Modalidad,
            CodigoSistema = request.CodigoSistema,
            CodigoSucursal = request.CodigoSucursal,
            CodigoTipoPuntoVenta = request.CodigoTipoPuntoVenta,
            Cuis = request.Cuis,
            Descripcion = request.Descripcion,
            Nit = request.Nit,
            NombrePuntoVenta = request.NombrePuntoVenta,
        };

        var xml = SoapEnvelopeBuilder.BuildEnvelope("registroPuntoVenta", payload);

        var response = await client.PostSoapXmlAsync<RegistroPuntoVentaSoapResponse>(
            "FacturacionOperaciones", xml, token, request.Ambiente, payloadOnly: true, ct);

        return new RegistroPuntoVentaResponse
        {
            Transaccion = response.Transaccion,
            CodigoPuntoVenta = response.CodigoPuntoVenta,
            Mensajes = response.MensajesList
                .Select(m => new SiatMensaje { Codigo = m.Codigo, Descripcion = m.Descripcion })
                .ToList(),
        };
    }
}

[XmlRoot("SolicitudRegistroPuntoVenta")]
public sealed class RegistroPuntoVentaSoap
{
    [XmlElement("codigoAmbiente")]
    public int CodigoAmbiente { get; set; }

    [XmlElement("codigoModalidad")]
    public int CodigoModalidad { get; set; }

    [XmlElement("codigoSistema")]
    public string CodigoSistema { get; set; } = string.Empty;

    [XmlElement("codigoSucursal")]
    public int CodigoSucursal { get; set; }

    [XmlElement("codigoTipoPuntoVenta")]
    public int CodigoTipoPuntoVenta { get; set; }

    [XmlElement("cuis")]
    public string Cuis { get; set; } = string.Empty;

    [XmlElement("descripcion")]
    public string Descripcion { get; set; } = string.Empty;

    [XmlElement("nit")]
    public long Nit { get; set; }

    [XmlElement("nombrePuntoVenta")]
    public string NombrePuntoVenta { get; set; } = string.Empty;
}

[XmlRoot("RespuestaRegistroPuntoVenta")]
public sealed class RegistroPuntoVentaSoapResponse
{
    [XmlElement("codigoPuntoVenta", Namespace = "")]
    public int CodigoPuntoVenta { get; set; }

    [XmlElement("mensajesList", Namespace = "")]
    public List<SiatMensajeSoap> MensajesList { get; set; } = [];

    [XmlElement("transaccion", Namespace = "")]
    public bool Transaccion { get; set; }
}
