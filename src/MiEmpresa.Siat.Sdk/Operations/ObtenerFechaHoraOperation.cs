namespace MiEmpresa.Siat.Sdk.Operations;

using System.Xml.Serialization;

using MiEmpresa.Siat.Sdk.Clients;
using MiEmpresa.Siat.Sdk.Models;
using MiEmpresa.Siat.Sdk.Models.Requests;
using MiEmpresa.Siat.Sdk.Models.Responses;
using MiEmpresa.Siat.Sdk.Xml;

/// <summary>
/// Obtiene la fecha y hora oficial del SIN (`sincronizarFechaHora`). Se usa para emitir
/// con una `fechaEmision` que el SIN considere válida, sin depender del reloj local.
/// </summary>
public sealed class ObtenerFechaHoraOperation(ISiatClient client)
{
    public async Task<FechaHoraResponse> EjecutarAsync(
        ParametricaRequest request, string token, CancellationToken ct)
    {
        var xml = SoapEnvelopeBuilder.BuildEnvelope(
            "sincronizarFechaHora", ParametricaSolicitudSoap.From(request));

        var resp = await client.PostSoapXmlAsync<FechaHoraSoapResponse>(
            "FacturacionSincronizacion", xml, token, request.Ambiente, payloadOnly: true, ct);

        return new FechaHoraResponse
        {
            Transaccion = resp.Transaccion,
            FechaHora = SiatDate.ToUtc(resp.FechaHora),
        };
    }
}

[XmlRoot("RespuestaFechaHora")]
public sealed class FechaHoraSoapResponse
{
    [XmlElement("transaccion", Namespace = "")]
    public bool Transaccion { get; set; }

    [XmlElement("fechaHora", Namespace = "")]
    public DateTime FechaHora { get; set; }
}
