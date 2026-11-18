namespace MiEmpresa.Siat.Sdk.Xml;

using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

public static class SoapEnvelopeBuilder
{
    private static readonly XmlSerializerNamespaces _namespaces;
    private static readonly ConcurrentDictionary<Type, XmlSerializer> _serializers = new();

    static SoapEnvelopeBuilder()
    {
        _namespaces = new XmlSerializerNamespaces();
        _namespaces.Add("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
        _namespaces.Add("ns", "https://siat.impuestos.gob.bo/");
    }

    public static SoapEnvelope<T> Build<T>(T bodyContent) where T : class
    {
        return new SoapEnvelope<T>
        {
            Body = new SoapBody<T> { Content = bodyContent }
        };
    }

    public static string Serialize<T>(SoapEnvelope<T> envelope) where T : class
    {
        var serializer = GetOrCreateSerializer(typeof(SoapEnvelope<T>));
        
        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = false,
            Encoding = Encoding.UTF8
        });

        serializer.Serialize(xmlWriter, envelope, _namespaces);
        return stringWriter.ToString();
    }

    public static SoapEnvelope<T> Deserialize<T>(string xml) where T : class
    {
        var serializer = GetOrCreateSerializer(typeof(SoapEnvelope<T>));
        
        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader);
        return (SoapEnvelope<T>)serializer.Deserialize(xmlReader)!;
    }

    private static XmlSerializer GetOrCreateSerializer(Type type)
    {
        return _serializers.GetOrAdd(type, t => new XmlSerializer(t, new XmlRootAttribute("Envelope")));
    }
}

[XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public sealed class SoapEnvelope<T> where T : class
{
    [XmlNamespaceDeclarations]
    public XmlSerializerNamespaces Namespaces { get; set; } = new();

    [XmlElement("Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public SoapBody<T> Body { get; set; } = new();
}

public sealed class SoapBody<T> where T : class
{
    [XmlElement("Content", Namespace = "https://siat.impuestos.gob.bo/")]
    public T? Content { get; set; }
}

[XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public sealed class SoapEnvelopeResponse<T>
{
    [XmlElement("Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public SoapBodyResponse<T> Body { get; set; } = new();
}

public sealed class SoapBodyResponse<T>
{
    [XmlElement("Content", Namespace = "https://siat.impuestos.gob.bo/")]
    public T? Content { get; set; }
}