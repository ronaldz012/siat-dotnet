namespace MiEmpresa.Siat.Sdk.Operations;

using System.Xml.Serialization;

using MiEmpresa.Siat.Sdk.Clients;
using MiEmpresa.Siat.Sdk.Models.Requests;
using MiEmpresa.Siat.Sdk.Models.Responses;
using MiEmpresa.Siat.Sdk.Xml;

/// <summary>
/// Catálogos de <c>FacturacionSincronizacion</c> cuya respuesta NO es la paramétrica
/// estándar `RespuestaListaParametricas`: actividades, productos/servicios, leyendas y
/// actividades por documento sector. Cada uno tiene su propio payload de respuesta.
/// </summary>
public sealed class SincronizarCatalogosEspecialesOperation(ISiatClient client)
{
    public async Task<CatalogoResponse<ActividadEconomicaSiat>> ActividadesAsync(
        ParametricaRequest request, string token, CancellationToken ct)
    {
        var resp = await EnviarAsync<ActividadesSoapResponse>("sincronizarActividades", request, token, ct);

        return new CatalogoResponse<ActividadEconomicaSiat>
        {
            Transaccion = resp.Transaccion,
            Items = resp.ListaActividades
                .Select(a => new ActividadEconomicaSiat
                {
                    CodigoCaeb = a.CodigoCaeb,
                    Descripcion = a.Descripcion,
                    TipoActividad = a.TipoActividad,
                })
                .ToList(),
            Mensajes = Mensajes(resp.MensajesList),
        };
    }

    public async Task<CatalogoResponse<ProductoServicioSiat>> ProductosServiciosAsync(
        ParametricaRequest request, string token, CancellationToken ct)
    {
        var resp = await EnviarAsync<ProductosSoapResponse>(
            "sincronizarListaProductosServicios", request, token, ct);

        return new CatalogoResponse<ProductoServicioSiat>
        {
            Transaccion = resp.Transaccion,
            Items = resp.ListaCodigos
                .Select(p => new ProductoServicioSiat
                {
                    CodigoActividad = p.CodigoActividad,
                    CodigoProducto = p.CodigoProducto,
                    DescripcionProducto = p.DescripcionProducto,
                })
                .ToList(),
            Mensajes = Mensajes(resp.MensajesList),
        };
    }

    public async Task<CatalogoResponse<LeyendaFacturaSiat>> LeyendasFacturaAsync(
        ParametricaRequest request, string token, CancellationToken ct)
    {
        var resp = await EnviarAsync<LeyendasSoapResponse>(
            "sincronizarListaLeyendasFactura", request, token, ct);

        return new CatalogoResponse<LeyendaFacturaSiat>
        {
            Transaccion = resp.Transaccion,
            Items = resp.ListaLeyendas
                .Select(l => new LeyendaFacturaSiat
                {
                    CodigoActividad = l.CodigoActividad,
                    DescripcionLeyenda = l.DescripcionLeyenda,
                })
                .ToList(),
            Mensajes = Mensajes(resp.MensajesList),
        };
    }

    public async Task<CatalogoResponse<ActividadDocumentoSectorSiat>> ActividadesDocumentoSectorAsync(
        ParametricaRequest request, string token, CancellationToken ct)
    {
        var resp = await EnviarAsync<ActividadesDocumentoSectorSoapResponse>(
            "sincronizarListaActividadesDocumentoSector", request, token, ct);

        return new CatalogoResponse<ActividadDocumentoSectorSiat>
        {
            Transaccion = resp.Transaccion,
            Items = resp.ListaActividadesDocumentoSector
                .Select(a => new ActividadDocumentoSectorSiat
                {
                    CodigoActividad = a.CodigoActividad,
                    CodigoDocumentoSector = a.CodigoDocumentoSector,
                    TipoDocumentoSector = a.TipoDocumentoSector,
                })
                .ToList(),
            Mensajes = Mensajes(resp.MensajesList),
        };
    }

    private async Task<TResponse> EnviarAsync<TResponse>(
        string operacion, ParametricaRequest request, string token, CancellationToken ct)
        where TResponse : class
    {
        var xml = SoapEnvelopeBuilder.BuildEnvelope(operacion, ParametricaSolicitudSoap.From(request));

        return await client.PostSoapXmlAsync<TResponse>(
            "FacturacionSincronizacion", xml, token, request.Ambiente, payloadOnly: true, ct);
    }

    private static List<SiatMensaje> Mensajes(IEnumerable<SiatMensajeSoap> mensajes) =>
        mensajes.Select(m => new SiatMensaje { Codigo = m.Codigo, Descripcion = m.Descripcion }).ToList();
}

// ---------- Payloads SOAP de respuesta (nombres y campos del WSDL del SIN) ----------

[XmlRoot("RespuestaListaActividades")]
public sealed class ActividadesSoapResponse
{
    [XmlElement("transaccion", Namespace = "")]
    public bool Transaccion { get; set; }

    [XmlElement("listaActividades", Namespace = "")]
    public List<ActividadSoap> ListaActividades { get; set; } = [];

    [XmlElement("mensajesList", Namespace = "")]
    public List<SiatMensajeSoap> MensajesList { get; set; } = [];
}

public sealed class ActividadSoap
{
    [XmlElement("codigoCaeb", Namespace = "")]
    public string CodigoCaeb { get; set; } = string.Empty;

    [XmlElement("descripcion", Namespace = "")]
    public string Descripcion { get; set; } = string.Empty;

    [XmlElement("tipoActividad", Namespace = "")]
    public string TipoActividad { get; set; } = string.Empty;
}

[XmlRoot("RespuestaListaProductos")]
public sealed class ProductosSoapResponse
{
    [XmlElement("transaccion", Namespace = "")]
    public bool Transaccion { get; set; }

    [XmlElement("listaCodigos", Namespace = "")]
    public List<ProductoSoap> ListaCodigos { get; set; } = [];

    [XmlElement("mensajesList", Namespace = "")]
    public List<SiatMensajeSoap> MensajesList { get; set; } = [];
}

public sealed class ProductoSoap
{
    [XmlElement("codigoActividad", Namespace = "")]
    public string CodigoActividad { get; set; } = string.Empty;

    [XmlElement("codigoProducto", Namespace = "")]
    public int CodigoProducto { get; set; }

    [XmlElement("descripcionProducto", Namespace = "")]
    public string DescripcionProducto { get; set; } = string.Empty;
}

[XmlRoot("RespuestaListaParametricasLeyendas")]
public sealed class LeyendasSoapResponse
{
    [XmlElement("transaccion", Namespace = "")]
    public bool Transaccion { get; set; }

    [XmlElement("listaLeyendas", Namespace = "")]
    public List<LeyendaSoap> ListaLeyendas { get; set; } = [];

    [XmlElement("mensajesList", Namespace = "")]
    public List<SiatMensajeSoap> MensajesList { get; set; } = [];
}

public sealed class LeyendaSoap
{
    [XmlElement("codigoActividad", Namespace = "")]
    public string CodigoActividad { get; set; } = string.Empty;

    [XmlElement("descripcionLeyenda", Namespace = "")]
    public string DescripcionLeyenda { get; set; } = string.Empty;
}

[XmlRoot("RespuestaListaActividadesDocumentoSector")]
public sealed class ActividadesDocumentoSectorSoapResponse
{
    [XmlElement("transaccion", Namespace = "")]
    public bool Transaccion { get; set; }

    [XmlElement("listaActividadesDocumentoSector", Namespace = "")]
    public List<ActividadDocumentoSectorSoap> ListaActividadesDocumentoSector { get; set; } = [];

    [XmlElement("mensajesList", Namespace = "")]
    public List<SiatMensajeSoap> MensajesList { get; set; } = [];
}

public sealed class ActividadDocumentoSectorSoap
{
    [XmlElement("codigoActividad", Namespace = "")]
    public string CodigoActividad { get; set; } = string.Empty;

    [XmlElement("codigoDocumentoSector", Namespace = "")]
    public int CodigoDocumentoSector { get; set; }

    [XmlElement("tipoDocumentoSector", Namespace = "")]
    public string TipoDocumentoSector { get; set; } = string.Empty;
}
