namespace MiEmpresa.Siat.Sdk.Clients;

using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiEmpresa.Siat.Sdk.Options;
using MiEmpresa.Siat.Sdk.Exceptions;
using MiEmpresa.Siat.Sdk.Xml;

public interface ISiatClient
{
    Task<TResponse> PostSoapAsync<TRequest, TResponse>(
        string serviceName,
        TRequest request,
        string token,
        CancellationToken ct)
        where TRequest : class
        where TResponse : class;
}

public sealed class SiatHttpClient : ISiatClient
{
    private readonly HttpClient _httpClient;
    private readonly SiatOptions _options;
    private readonly ILogger<SiatHttpClient>? _logger;

    public SiatHttpClient(
        HttpClient httpClient,
        IOptions<SiatOptions> options,
        ILogger<SiatHttpClient>? logger = null)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<TResponse> PostSoapAsync<TRequest, TResponse>(
        string serviceName,
        TRequest request,
        string token,
        CancellationToken ct)
        where TRequest : class
        where TResponse : class
    {
        var baseUrl = _options.GetBaseUrl(_options.DefaultAmbiente);
        var url = $"{baseUrl}/{serviceName}";

        var soapEnvelope = SoapEnvelopeBuilder.Build(request);
        var xmlContent = SoapEnvelopeBuilder.Serialize(soapEnvelope);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(xmlContent, Encoding.UTF8, "application/xml")
        };

        httpRequest.Headers.Add("apiKey", $"TokenApi {token}");
        httpRequest.Headers.UserAgent.ParseAdd(_options.UserAgent);

        _logger?.LogDebug("Enviando SOAP a {Url}: {Xml}", url, xmlContent);

        var response = await _httpClient.SendAsync(httpRequest, ct);

        var responseXml = await response.Content.ReadAsStringAsync(ct);

        _logger?.LogDebug("Respuesta SOAP de {Url}: {Xml}", url, responseXml);

        if (!response.IsSuccessStatusCode)
        {
            throw new SiatCommunicationException(
                $"Error HTTP {response.StatusCode} al llamar {serviceName}",
                httpStatusCode: (int)response.StatusCode);
        }

        // Extraer el contenido del Body de la respuesta SOAP
        var innerXml = ExtraerContenidoBody(responseXml);

        var serializer = new XmlSerializer(typeof(TResponse));
        using var stringReader = new StringReader(innerXml);
        using var xmlReader = XmlReader.Create(stringReader);
        var obj = serializer.Deserialize(xmlReader);
        return obj is TResponse parsed
            ? parsed
            : throw new SiatXmlException($"La respuesta de {serviceName} no se pudo interpretar como {typeof(TResponse).Name}.");
    }

    private static string ExtraerContenidoBody(string soapResponse)
    {
        try
        {
            var doc = XDocument.Parse(soapResponse);
            var nsSoap = XNamespace.Get("http://schemas.xmlsoap.org/soap/envelope/");
            var body = doc.Descendants(nsSoap + "Body").FirstOrDefault();
            
            if (body != null)
            {
                // Retornar el primer elemento hijo del Body como XML string
                var firstChild = body.Elements().FirstOrDefault();
                if (firstChild != null)
                {
                    return firstChild.ToString();
                }
            }
        }
        catch
        {
            // Si falla el parsing, intentar devolver el XML tal cual
        }
        
        return soapResponse;
    }
}