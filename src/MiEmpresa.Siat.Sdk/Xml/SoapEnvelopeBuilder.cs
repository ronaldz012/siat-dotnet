namespace MiEmpresa.Siat.Sdk.Xml;

using System.Collections.Concurrent;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using MiEmpresa.Siat.Sdk.Exceptions;

/// <summary>
/// Construye el sobre SOAP 1.1 del SIAT. El Body lleva DIRECTAMENTE el elemento de la
/// operación (ej. &lt;ns:cuis&gt;), como lo define el WSDL: no hay wrapper propio.
/// </summary>
public static class SoapEnvelopeBuilder
{
    public const string SiatNamespace = "https://siat.impuestos.gob.bo/";
    public const string SoapNamespace = "http://schemas.xmlsoap.org/soap/envelope/";

    private static readonly ConcurrentDictionary<Type, XmlSerializer> _serializers = new();

    /// <summary>Serializa el request (que ya declara su elemento de operación) y lo envuelve.</summary>
    public static string BuildEnvelope<TRequest>(TRequest request) where TRequest : class
        => Wrap(SerializeElement(request, withNsPrefix: true));

    /// <summary>
    /// Para operaciones cuyo nombre es dinámico (catálogos de sincronización): envuelve el
    /// payload en &lt;ns:{operacion}&gt;. El payload se serializa sin namespace.
    /// </summary>
    public static string BuildEnvelope(string operacion, object payload)
        => Wrap($"<ns:{operacion}>{SerializeElement(payload, withNsPrefix: false)}</ns:{operacion}>");

    /// <summary>XML del primer elemento del Body: la respuesta de la operación.</summary>
    public static string ExtractBodyContent(string soapResponse)
    {
        var doc = ParseEnvelope(soapResponse);
        var body = doc.Descendants(XName.Get("Body", SoapNamespace)).FirstOrDefault();
        var content = body?.Elements().FirstOrDefault()
            ?? throw new SiatXmlException("La respuesta del SIN no trae contenido en el Body SOAP.");

        // El SIN a veces manda el wrapper con namespace por defecto (los hijos heredan)
        // y a veces con prefijo (los hijos quedan sin namespace). Normalizamos a sin
        // namespace para que el parseo no dependa de esa variante.
        return StripNamespaces(content).ToString();
    }

    /// <summary>
    /// Primer hijo del wrapper de respuesta (ej. &lt;RespuestaListaParametricas&gt;). Útil
    /// para operaciones donde el nombre del wrapper varía pero el payload es el mismo.
    /// </summary>
    public static string ExtractPayloadContent(string soapResponse)
    {
        var wrapper = XDocument.Parse(ExtractBodyContent(soapResponse));
        var payload = wrapper.Root?.Elements().FirstOrDefault()
            ?? throw new SiatXmlException("La respuesta del SIN no trae el payload esperado.");

        return payload.ToString();
    }

    /// <summary>True si la respuesta trae &lt;transaccion&gt;false&lt;/transaccion&gt;.</summary>
    public static bool IsUnsuccessful(string soapResponse)
    {
        try
        {
            return XDocument.Parse(soapResponse)
                .Descendants()
                .Any(e => e.Name.LocalName == "transaccion"
                    && string.Equals(e.Value.Trim(), "false", StringComparison.OrdinalIgnoreCase));
        }
        catch (XmlException)
        {
            return false;
        }
    }

    /// <summary>SOAP Fault de la respuesta (código + mensaje + detalle), o null si no hay.</summary>
    public static SiatFault? TryReadFault(string soapResponse)
    {
        XDocument doc;
        try
        {
            doc = XDocument.Parse(soapResponse);
        }
        catch (XmlException)
        {
            return null;
        }

        var fault = doc.Descendants(XName.Get("Fault", SoapNamespace)).FirstOrDefault();
        if (fault is null)
            return null;

        return new SiatFault(
            fault.Element(XName.Get("faultcode"))?.Value ?? "soap:Fault",
            fault.Element(XName.Get("faultstring"))?.Value ?? "(sin mensaje)",
            fault.Element(XName.Get("detail"))?.Value);
    }

    private static string SerializeElement(object request, bool withNsPrefix)
    {
        var serializer = _serializers.GetOrAdd(request.GetType(), t => new XmlSerializer(t));

        var namespaces = new XmlSerializerNamespaces();
        if (withNsPrefix)
            namespaces.Add("ns", SiatNamespace);
        else
            namespaces.Add(string.Empty, string.Empty); // evita los xmlns:xsi/xsd

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { OmitXmlDeclaration = true });
        serializer.Serialize(xmlWriter, request, namespaces);

        return stringWriter.ToString();
    }

    private static string Wrap(string body) =>
        $"""
        <?xml version="1.0" encoding="utf-8"?>
        <soapenv:Envelope xmlns:soapenv="{SoapNamespace}" xmlns:ns="{SiatNamespace}">
          <soapenv:Header />
          <soapenv:Body>
            {body}
          </soapenv:Body>
        </soapenv:Envelope>
        """;

    private static XDocument ParseEnvelope(string xml)
    {
        try
        {
            return XDocument.Parse(xml);
        }
        catch (XmlException ex)
        {
            throw new SiatXmlException("La respuesta del SIN no es XML válido.", ex);
        }
    }

    private static XElement StripNamespaces(XElement element)
    {
        var content = new List<object>();

        foreach (var attribute in element.Attributes().Where(a => !a.IsNamespaceDeclaration))
            content.Add(new XAttribute(attribute.Name.LocalName, attribute.Value));

        foreach (var node in element.Nodes())
            content.Add(node is XElement child ? StripNamespaces(child) : node);

        return new XElement(element.Name.LocalName, content);
    }
}

/// <summary>Fault devuelto por el SIN dentro del sobre SOAP.</summary>
public sealed record SiatFault(string Code, string Message, string? Detail)
{
    public override string ToString() =>
        string.IsNullOrWhiteSpace(Detail) ? $"{Code}: {Message}" : $"{Code}: {Message} ({Detail})";
}
