namespace MiEmpresa.Siat.Sdk.Operations;
using System.Xml.Serialization;

using MiEmpresa.Siat.Sdk.Clients;
using MiEmpresa.Siat.Sdk.Models;
using MiEmpresa.Siat.Sdk.Models.Requests;
using MiEmpresa.Siat.Sdk.Models.Responses;
using MiEmpresa.Siat.Sdk.Xml;

/// <summary>
/// Sincroniza una paramétrica (catálogo) estándar de <c>FacturacionSincronizacion</c>.
/// El enum <see cref="ParametricaSiat"/> selecciona la operación SOAP; el payload y la
/// respuesta son los mismos para todos (código/descripción).
/// </summary>
public sealed class SincronizarParametricaOperation
{
    private readonly ISiatClient _client;

    public SincronizarParametricaOperation(ISiatClient client)
    {
        _client = client;
    }

    public async Task<SincronizacionParametricaResponse> EjecutarAsync(
        ParametricaSiat catalogo, ParametricaRequest request, string token, CancellationToken ct)
    {
        var payload = ParametricaSolicitudSoap.From(request);

        var xml = SoapEnvelopeBuilder.BuildEnvelope(Operacion(catalogo), payload);

        var response = await _client.PostSoapXmlAsync<ParametricaSoapResponse>(
            "FacturacionSincronizacion",
            xml,
            token,
            request.Ambiente,
            payloadOnly: true,
            ct);

        return new SincronizacionParametricaResponse
        {
            Transaccion = response.Transaccion,
            Parametros = response.ListaCodigos
                .Select(p => new ParametroSiat { Codigo = p.CodigoClasificador, Descripcion = p.Descripcion })
                .ToList(),
            Mensajes = response.MensajesList
                .Select(m => new SiatMensaje { Codigo = m.Codigo, Descripcion = m.Descripcion })
                .ToList(),
        };
    }

    /// <summary>Nombre de la operación SOAP (contrato del SIN; no lo conocen los consumidores).</summary>
    private static string Operacion(ParametricaSiat catalogo) => catalogo switch
    {
        ParametricaSiat.TipoPuntoVenta => "sincronizarParametricaTipoPuntoVenta",
        ParametricaSiat.UnidadMedida => "sincronizarParametricaUnidadMedida",
        ParametricaSiat.TipoDocumentoIdentidad => "sincronizarParametricaTipoDocumentoIdentidad",
        ParametricaSiat.TipoMetodoPago => "sincronizarParametricaTipoMetodoPago",
        ParametricaSiat.TipoMoneda => "sincronizarParametricaTipoMoneda",
        ParametricaSiat.TiposFactura => "sincronizarParametricaTiposFactura",
        ParametricaSiat.TipoDocumentoSector => "sincronizarParametricaTipoDocumentoSector",
        ParametricaSiat.TipoEmision => "sincronizarParametricaTipoEmision",
        ParametricaSiat.MotivoAnulacion => "sincronizarParametricaMotivoAnulacion",
        ParametricaSiat.EventosSignificativos => "sincronizarParametricaEventosSignificativos",
        ParametricaSiat.PaisOrigen => "sincronizarParametricaPaisOrigen",
        ParametricaSiat.TipoHabitacion => "sincronizarParametricaTipoHabitacion",
        ParametricaSiat.MensajesServicios => "sincronizarListaMensajesServicios",
        _ => throw new ArgumentOutOfRangeException(nameof(catalogo), catalogo, "Paramétrica sin operación SOAP."),
    };
}

/// <summary>Payload &lt;SolicitudSincronizacion&gt; (sin namespace; el prefijo lo pone la operación).</summary>
[XmlRoot("SolicitudSincronizacion")]
public sealed class ParametricaSolicitudSoap
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

    public static ParametricaSolicitudSoap From(ParametricaRequest request) => new()
    {
        CodigoAmbiente = (int)request.Ambiente,
        CodigoPuntoVenta = request.CodigoPuntoVenta,
        CodigoSistema = request.CodigoSistema,
        CodigoSucursal = request.CodigoSucursal,
        Cuis = request.Cuis,
        Nit = request.Nit,
    };
}

/// <summary>&lt;RespuestaListaParametricas&gt; común a los catálogos estándar.</summary>
[XmlRoot("RespuestaListaParametricas")]
public sealed class ParametricaSoapResponse
{
    [XmlElement("transaccion", Namespace = "")]
    public bool Transaccion { get; set; }

    [XmlElement("listaCodigos", Namespace = "")]
    public List<ParametroSoap> ListaCodigos { get; set; } = [];

    [XmlElement("mensajesList", Namespace = "")]
    public List<SiatMensajeSoap> MensajesList { get; set; } = [];
}

public sealed class ParametroSoap
{
    [XmlElement("codigoClasificador", Namespace = "")]
    public int CodigoClasificador { get; set; }

    [XmlElement("descripcion", Namespace = "")]
    public string Descripcion { get; set; } = string.Empty;
}
